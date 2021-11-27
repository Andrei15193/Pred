using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pred.Expressions;

namespace Pred
{
    public class PredicateProcessor
    {
        private readonly IPredicateProvider _predicateProvider;

        public PredicateProcessor(IPredicateProvider predicateProvider)
            => _predicateProvider = predicateProvider;

        public PredicateProcessor(IEnumerable<Predicate> predicates)
            : this(new InMemoryPredicateProvider(predicates))
        {
        }

        public PredicateProcessor(params Predicate[] predicates)
            : this((IEnumerable<Predicate>)predicates)
        {
        }

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameter)
            => ProcessAsync(predicateName, parameter, CancellationToken.None);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, params CallParameter[] parameters)
            => ProcessAsync(predicateName, parameters, CancellationToken.None);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameters, CancellationToken cancellationToken)
        {
            if (predicateName is null)
                throw new ArgumentNullException(nameof(predicateName));
            var parametersList = parameters?.ToArray();
            if (parameters is null || parametersList.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            return _ProcessAsync(predicateName, parametersList, cancellationToken);
        }

        private async IAsyncEnumerable<PredicateProcessResult> _ProcessAsync(string predicateName, IReadOnlyList<CallParameter> callParameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pendingPredicateProviders = new Queue<ProcessorPredicateProvider>();
            pendingPredicateProviders.Enqueue(new ProcessorPredicateProvider(cancellationToken => _predicateProvider.GetPredicatesAsync(predicateName, cancellationToken)));

            do
            {
                var predicateProvider = pendingPredicateProviders.Dequeue();
                await foreach (var predicate in predicateProvider.GetPredicatesAsync(cancellationToken).WithCancellation(cancellationToken))
                    if (_AreParametersMatching(callParameters, predicate.Parameters))
                    {
                        var context = new PredicateProcessorContext(predicate, callParameters, predicateProvider.BaseContext);

                        var isPredicateTrue = true;
                        while (isPredicateTrue && context.NextExpression())
                            switch (context.CurrentExpression)
                            {
                                case BindOrCheckPredicateExpression bindOrCheckExpression:
                                    isPredicateTrue = _Visit(context, bindOrCheckExpression);
                                    break;

                                case CallPredicateExpression callExpression:
                                    isPredicateTrue = _Visit(context, callExpression);
                                    break;

                                default:
                                    throw new NotImplementedException($"Unhandled expression type '{context.CurrentExpression.GetType()}'.");
                            }

                        foreach (var additionalPredicateProvider in context.AdditionalPredicateProviders)
                            pendingPredicateProviders.Enqueue(additionalPredicateProvider);

                        if (isPredicateTrue)
                        {
                            foreach (var localCallParameter in context.ResultParameterMapping.Keys.Except(callParameters).ToList())
                            {
                                var resultParameter = context.ResultParameterMapping[localCallParameter];
                                resultParameter.UnbindParameter(localCallParameter);
                                context.ResultParameterMapping.Remove(localCallParameter);
                            }
                            yield return new PredicateProcessResult(callParameters.Select(callParameter => (callParameter, context.ResultParameterMapping[callParameter])));
                        }
                    }
            } while (pendingPredicateProviders.Count > 0);
        }

        private bool _Visit(PredicateProcessorContext context, BindOrCheckPredicateExpression bindOrCheckExpression)
        {
            var callParameter = _GetCallParameter(context, bindOrCheckExpression.Parameter);
            var resultParameter = context.ResultParameterMapping[callParameter];

            switch (bindOrCheckExpression.Value)
            {
                case ConstantPredicateExpression constantExpression:
                    if (resultParameter.IsBoundToValue)
                        return Equals(resultParameter.BoundValue, constantExpression.Value);
                    else
                    {
                        resultParameter.BindValue(constantExpression.Value);
                        return true;
                    }

                case ParameterPredicateExpression parameterExpression:
                    var otherCallParameter = _GetCallParameter(context, parameterExpression.Parameter); ;
                    var otherResultParameter = context.ResultParameterMapping[otherCallParameter];

                    if (resultParameter.IsBoundToValue && otherResultParameter.IsBoundToValue)
                        return Equals(resultParameter.BoundValue, otherResultParameter.BoundValue);
                    else
                    {
                        resultParameter.BindParameter(otherResultParameter);
                        context.ResultParameterMapping[otherCallParameter] = resultParameter;

                        if (otherResultParameter.IsBoundToValue)
                            resultParameter.BindValue(otherResultParameter.BoundValue);
                    }
                    return true;

                default:
                    throw new NotImplementedException($"Unhandled expression type '{bindOrCheckExpression.Value.GetType()}'.");
            }
        }

        private bool _Visit(PredicateProcessorContext context, CallPredicateExpression callExpression)
        {
            context.AddPredicateProvider(cancellationToken => _ProcessCallAsync(context, callExpression, cancellationToken));
            return false;
        }

        private async IAsyncEnumerable<Predicate> _ProcessCallAsync(PredicateProcessorContext context, CallPredicateExpression callExpression, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var invokeParameters = callExpression
                .Parameters
                .Select((parameter, parameterIndex) =>
                {
                    if (parameter is ParameterPredicateExpression parameterExpression)
                        return _GetCallParameter(context, parameterExpression.Parameter);
                    else if (parameter is ConstantPredicateExpression constantExpression)
                        return (CallParameter)typeof(Parameter)
                            .GetMethod(nameof(Parameter.Input), 1, new[] { typeof(string), Type.MakeGenericMethodParameter(0) })
                            .MakeGenericMethod(constantExpression.ValueType)
                            .Invoke(null, new[] { $"parameter{parameterIndex}", constantExpression.Value });
                    else
                        throw new InvalidOperationException($"Unhandled expression type '{parameter.GetType()}'.");
                })
                .ToArray();

            await foreach (var result in ProcessAsync(callExpression.Name, invokeParameters, cancellationToken).WithCancellation(cancellationToken))
            {
                var predicateBody = new List<PredicateExpression>();

                foreach (var invokeResultParameter in result)
                {
                    var matchingCallParameters = invokeResultParameter
                        .BoundParameters
                        .Where(context.ResultParameterMapping.ContainsKey);
                    if (matchingCallParameters.Any())
                    {
                        var firstMatchingCallParameter = matchingCallParameters.First();
                        if (invokeResultParameter.IsBoundToValue)
                            predicateBody.Add(new BindOrCheckPredicateExpression(
                                firstMatchingCallParameter,
                                (ConstantPredicateExpression)Activator.CreateInstance(typeof(ConstantPredicateExpression<>).MakeGenericType(firstMatchingCallParameter.ParameterType), new[] { invokeResultParameter.BoundValue })
                            ));
                        foreach (var matchingCallParameter in matchingCallParameters)
                            predicateBody.Add(new BindOrCheckPredicateExpression(matchingCallParameter, new ParameterPredicateExpression(firstMatchingCallParameter)));
                    }
                }
                predicateBody.AddRange(context.RemainingExpressions);

                yield return new Predicate(context.Predicate.Parameters, predicateBody);
            }
        }

        private static CallParameter _GetCallParameter(PredicateProcessorContext context, Parameter parameter)
        {
            if (parameter is PredicateParameter expressionPredicateParameter)
                return context.CallParameterMapping[expressionPredicateParameter];
            else if (parameter is CallParameter expressionCallParameter)
            {
                context.AddCallParameter(expressionCallParameter);
                return expressionCallParameter;
            }
            else
                throw new InvalidOperationException($"Unhandled parameter type '{parameter.GetType()}'.");
        }

        private static bool _AreParametersMatching(IReadOnlyList<CallParameter> callParameters, IReadOnlyList<Parameter> predicateParameters)
            => callParameters.Count == predicateParameters.Count
                && callParameters
                .Zip(predicateParameters, (callParameter, predicateParameter) => (CallParameter: callParameter, PredicateParameter: predicateParameter))
                .All(
                    pair => pair.CallParameter.IsInput
                        ? pair.PredicateParameter.ParameterType.IsAssignableFrom(pair.CallParameter.ParameterType)
                        : pair.CallParameter.IsOutput
                        ? pair.CallParameter.ParameterType.IsAssignableFrom(pair.PredicateParameter.ParameterType)
                        : false
                );
    }
}