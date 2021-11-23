using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
            await foreach (var predicate in _predicateProvider.GetPredicates(predicateName).WithCancellation(cancellationToken))
                if (_AreParametersMatching(callParameters, predicate.Parameters))
                {
                    var context = new
                    {
                        CallParameterMapping = (IReadOnlyDictionary<PredicateParameter, CallParameter>)callParameters
                            .Select((parameter, index) => (Parameter: parameter, Index: index))
                            .ToDictionary(pair => predicate.Parameters[pair.Index], pair => pair.Parameter),
                        ResultParameterMapping = (IDictionary<CallParameter, ResultParameter>)callParameters.ToDictionary(parameter => parameter, _CreateProcessResultParameter)
                    };

                    var isPredicateTrue = true;
                    using (var expression = predicate.Body.GetEnumerator())
                        while (isPredicateTrue && expression.MoveNext())
                            switch (expression.Current)
                            {
                                case BindOrCheckPredicateExpression bindOrCheckExpression:
                                    var callParameter = context.CallParameterMapping[bindOrCheckExpression.Parameter];
                                    var resultParameter = context.ResultParameterMapping[callParameter];

                                    switch (bindOrCheckExpression.Value)
                                    {
                                        case ConstantPredicateExpression constantExpression:
                                            if (resultParameter.IsBoundToValue)
                                                isPredicateTrue = Equals(resultParameter.BoundValue, constantExpression.Value);
                                            else
                                                resultParameter.BindValue(constantExpression.Value);
                                            break;

                                        case ParameterPredicateExpression parameterExpression:
                                            var matchingCallParameter = context.CallParameterMapping[parameterExpression.Parameter];
                                            var matchingResultParameter = context.ResultParameterMapping[matchingCallParameter];
                                            resultParameter.BindParameter(matchingResultParameter);
                                            foreach (var boundCallParameter in resultParameter.BoundParameters)
                                                context.ResultParameterMapping[boundCallParameter] = resultParameter;

                                            if (matchingResultParameter.IsBoundToValue)
                                                resultParameter.BindValue(matchingResultParameter.BoundValue);
                                            break;

                                        default:
                                            throw new NotImplementedException($"Unhandled expression type '{bindOrCheckExpression.Value.GetType()}'.");
                                    }
                                    break;

                                case CallPredicateExpression callExpression:
                                    var invokeParameters = callExpression
                                        .Parameters
                                        .Select((parameter, parameterIndex) =>
                                        {
                                            if (parameter is ParameterPredicateExpression parameterExpression)
                                                return context.CallParameterMapping[parameterExpression.Parameter];
                                            else if (parameter is ConstantPredicateExpression constantExpression)
                                                return (CallParameter)typeof(Parameter)
                                                    .GetMethod(nameof(Parameter.Input), 1, new[] { typeof(string), Type.MakeGenericMethodParameter(0) })
                                                    .MakeGenericMethod(constantExpression.ValueType)
                                                    .Invoke(null, new[] { $"parameter{parameterIndex}", constantExpression.Value });
                                            else
                                                throw new InvalidOperationException($"Unhandled expression type '{parameter.GetType()}'.");
                                        })
                                        .ToList();
                                    isPredicateTrue = false;
                                    await foreach (var result in ProcessAsync(callExpression.Name, invokeParameters, cancellationToken).WithCancellation(cancellationToken))
                                    {
                                        isPredicateTrue = true;
                                        foreach (var invokeResultParameter in context.ResultParameterMapping.Keys.Select(callParameter => result[callParameter]).Distinct())
                                        {
                                            var matchingBoundParameters = invokeResultParameter.BoundParameters.Where(context.ResultParameterMapping.ContainsKey);
                                            if (matchingBoundParameters.Any())
                                            {
                                                var firstBoundParameter = matchingBoundParameters.First();
                                                var matchingResultParameter = context.ResultParameterMapping[firstBoundParameter];
                                                if (invokeResultParameter.IsBoundToValue)
                                                    matchingResultParameter.BindValue(invokeResultParameter.BoundValue);
                                                foreach (var boundParameter in matchingBoundParameters.Skip(1))
                                                {
                                                    matchingResultParameter.BindParameter(context.ResultParameterMapping[boundParameter]);
                                                    context.ResultParameterMapping[boundParameter] = matchingResultParameter;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException($"Unhandled expression type '{expression.Current.GetType()}'.");
                            }

                    if (isPredicateTrue)
                        yield return new PredicateProcessResult(callParameters.Select(callParameter => (callParameter, context.ResultParameterMapping[callParameter])));
                }
        }

        private static ResultParameter _CreateProcessResultParameter(CallParameter callParameter)
        {
            var resultParameter = (ResultParameter)Activator.CreateInstance(
                typeof(ResultParameter<>).MakeGenericType(callParameter.ParameterType),
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                new[] { callParameter },
                CultureInfo.InvariantCulture
            );
            if (callParameter is InputParameter inputParameter)
                resultParameter.BindValue(inputParameter.Value);

            return resultParameter;
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