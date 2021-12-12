using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            var pendingPredicateProviders = new Queue<ProcessorPredicateProvider>();
            pendingPredicateProviders.Enqueue(new ProcessorPredicateProvider(cancellationToken => _predicateProvider.GetPredicatesAsync(predicateName, cancellationToken)));

            do
            {
                var predicateProvider = pendingPredicateProviders.Dequeue();
                await foreach (var predicate in predicateProvider.GetPredicatesAsync(cancellationToken).WithCancellation(cancellationToken))
                    if (_AreParametersMatching(callParameters, predicate.Parameters))
                    {
                        Debug.WriteLine($"{predicate.Name}({string.Join(", ", callParameters.Select(callParameter => callParameter.Name))})");
                        Debug.Indent();

                        var context = new PredicateProcessorContext(predicate, _predicateProvider, pendingPredicateProviders.Enqueue);

                        context.BeginVariableLifeCycle(predicate.Parameters.Select((parameter, parameterIndex) => new PredicateParameterMapping(parameter, callParameters[parameterIndex])));

                        var isPredicateTrue = true;
                        while (isPredicateTrue && context.NextExpression())
                        {
                            switch (context.CurrentExpression)
                            {
                                case BindOrCheckPredicateExpression bindOrCheckExpression:
                                    isPredicateTrue = _Visit(context, bindOrCheckExpression);
                                    break;

                                case CallPredicateExpression callExpression:
                                    isPredicateTrue = _Visit(context, callExpression);
                                    break;

                                case BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression:
                                    isPredicateTrue = _Visit(context, beginVariableLifeCycleExpression);
                                    break;

                                case EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression:
                                    isPredicateTrue = _Visit(context, endVariableLifeCycleExpression);
                                    break;

                                case CheckPredicateExpression checkExpression:
                                    isPredicateTrue = _Visit(context, checkExpression);
                                    break;

                                default:
                                    throw new NotImplementedException($"Unhandled expression type '{context.CurrentExpression.GetType()}'.");
                            }
                        }

                        Debug.Unindent();
                        Debug.WriteLine(isPredicateTrue ? "[complete]" : "[end]");

                        if (isPredicateTrue)
                        {
                            var variableLifeCycleContext = context.EndVariableLifeCycle();
                            yield return new PredicateProcessResult(callParameters.Select(callParameter => new ResultParameterMapping(callParameter, variableLifeCycleContext.ResultParameterMapping[callParameter])));
                        }
                    }
            } while (pendingPredicateProviders.Count > 0);
        }

        private static bool _Visit(PredicateProcessorContext context, BindOrCheckPredicateExpression bindOrCheckExpression)
        {
            var callParameter = _GetCallParameter(context, bindOrCheckExpression.Parameter);
            var resultParameter = context.VariableLifeCycleContext.ResultParameterMapping[callParameter];

            switch (bindOrCheckExpression.Value)
            {
                case ParameterPredicateExpression parameterExpression:
                    var otherCallParameter = _GetCallParameter(context, parameterExpression.Parameter);
                    var otherResultParameter = context.VariableLifeCycleContext.ResultParameterMapping[otherCallParameter];

                    Debug.WriteLine($"{callParameter.Name} = {otherCallParameter.Name}");

                    if (resultParameter.IsBoundToValue && otherResultParameter.IsBoundToValue)
                        return Equals(resultParameter.BoundValue, otherResultParameter.BoundValue);
                    else
                    {
                        resultParameter.BindParameter(otherResultParameter);
                        context.VariableLifeCycleContext.ResultParameterMapping[otherCallParameter] = resultParameter;

                        if (otherResultParameter.IsBoundToValue)
                            resultParameter.BindValue(otherResultParameter.BoundValue);
                    }
                    return true;

                case ValuePredicateExpression valueExpression:
                    Debug.Write($"{callParameter.Name} = ");
                    var value = _Evaluate(context, valueExpression);
                    Debug.WriteLine("");

                    if (resultParameter.IsBoundToValue)
                        return Equals(resultParameter.BoundValue, value);
                    else
                    {
                        resultParameter.BindValue(value);
                        return true;
                    }

                default:
                    throw new NotImplementedException($"Unhandled expression type '{bindOrCheckExpression.Value.GetType()}'.");
            }
        }

        private static object _Evaluate(PredicateProcessorContext context, ValuePredicateExpression valueExpression)
        {
            switch (valueExpression)
            {
                case ConstantPredicateExpression constantExpression:
                    Debug.Write(_GetValue(constantExpression.Value));
                    return constantExpression.Value;

                case MapPredicateExpression mapExpression:
                    var callParameter = _GetCallParameter(context, mapExpression.ParameterExpression.Parameter);
                    var resultParameter = context.VariableLifeCycleContext.ResultParameterMapping[callParameter];
                    if (resultParameter.IsBoundToValue)
                    {
                        var mapResult = mapExpression.Selector(resultParameter.BoundValue);
                        Debug.Write($"[callback] {_GetValue(mapResult)}");
                        return mapResult;
                    }
                    else
                        throw new InvalidOperationException($"Invalid map expression, parameter '{callParameter.Name}' is not bound to a value.");

                default:
                    throw new NotImplementedException($"Unhandled expression type '{valueExpression.GetType()}'.");
            }

            static string _GetValue(object value)
            {
                switch (value)
                {
                    case string @string:
                        return $"\"{@string.Replace("\"", "\\\"")}\"";

                    case IEnumerable collection:
                        return $"[ {string.Join(", ", collection.OfType<object>().Select(_GetValue))} ]";

                    case null:
                        return "null";

                    default:
                        return Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }
        }

        private static bool _Visit(PredicateProcessorContext context, CallPredicateExpression callExpression)
        {
            var invokeParameters = callExpression
                .Parameters
                .Select((parameter, parameterIndex) =>
                {
                    if (parameter is ParameterPredicateExpression parameterExpression)
                        return _GetCallParameter(context, parameterExpression.Parameter);
                    else if (parameter is ConstantPredicateExpression constantExpression)
                        return Parameter.Input(constantExpression.ValueType, constantExpression.Value);
                    else
                        throw new InvalidOperationException($"Unhandled expression type '{parameter.GetType()}'.");
                })
                .ToArray();

            Debug.WriteLine($"{callExpression.Name}({string.Join(", ", invokeParameters.Select(callParameter => callParameter.Name ?? ((InputParameter)callParameter).Value))})");

            context.AddPredicateProvider(cancellationToken => _ProcessCallAsync(context, callExpression.Name, invokeParameters, cancellationToken));
            return false;
        }

        private static async IAsyncEnumerable<Predicate> _ProcessCallAsync(PredicateProcessorContext context, string predicateName, IReadOnlyList<CallParameter> invokeParameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var variableLifeCycleContext = context.VariableLifeCycleContext;

            var invokeParameterBindingExpressions = variableLifeCycleContext.ResultParameterMapping.Values.Distinct().Aggregate(
                new List<PredicateExpression>(variableLifeCycleContext.ResultParameterMapping.Count),
                (invokeParameterBindingExpressions, resultParameter) =>
                {
                    var commonCallParameters = resultParameter.BoundParameters.Where(invokeParameters.Contains);
                    var firstCommonCallParameter = commonCallParameters.FirstOrDefault();
                    if (firstCommonCallParameter is object)
                    {
                        if (firstCommonCallParameter is OutputParameter && resultParameter.IsBoundToValue)
                            invokeParameterBindingExpressions.Add(new BindOrCheckPredicateExpression(
                                firstCommonCallParameter,
                                (ConstantPredicateExpression)typeof(PredicateExpression)
                                    .GetMethod(nameof(PredicateExpression.Constant), 1, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, CallingConventions.Standard, new[] { Type.MakeGenericMethodParameter(0) }, null)
                                    .MakeGenericMethod(firstCommonCallParameter.ParameterType)
                                    .Invoke(null, new[] { resultParameter.BoundValue })
                            ));
                        foreach (var otherCommonCallParameter in commonCallParameters.Skip(1))
                            invokeParameterBindingExpressions.Add(new BindOrCheckPredicateExpression(firstCommonCallParameter, new ParameterPredicateExpression(otherCommonCallParameter)));
                    }
                    return invokeParameterBindingExpressions;
                }
            );

            var parameterBindingExpressions = variableLifeCycleContext.ResultParameterMapping.Values.Distinct().Aggregate(
                new List<PredicateExpression>(variableLifeCycleContext.ResultParameterMapping.Count),
                (parameterBindingExpressions, resultParameter) =>
                {
                    var firstBoundParameter = resultParameter.BoundParameters.FirstOrDefault();
                    if (firstBoundParameter is object)
                    {
                        if (resultParameter.IsBoundToValue)
                            parameterBindingExpressions.Add(new BindOrCheckPredicateExpression(
                                firstBoundParameter,
                                (ConstantPredicateExpression)typeof(PredicateExpression)
                                    .GetMethod(nameof(PredicateExpression.Constant), 1, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, CallingConventions.Standard, new[] { Type.MakeGenericMethodParameter(0) }, null)
                                    .MakeGenericMethod(firstBoundParameter.ParameterType)
                                    .Invoke(null, new[] { resultParameter.BoundValue })
                            ));
                        foreach (var otherCallParameter in resultParameter.BoundParameters.Skip(1))
                            parameterBindingExpressions.Add(new BindOrCheckPredicateExpression(firstBoundParameter, new ParameterPredicateExpression(otherCallParameter)));
                    }
                    return parameterBindingExpressions;
                }
            );

            await foreach (var predicate in context.PredicateProvider.GetPredicatesAsync(predicateName, cancellationToken).WithCancellation(cancellationToken))
                if (_AreParametersMatching(invokeParameters, predicate.Parameters))
                    yield return new Predicate(
                        context.Predicate.Parameters,
                        Enumerable
                            .Empty<PredicateExpression>()
                            .Append(new BeginVariableLifeCyclePredicateExpression(predicate.Parameters.Zip(invokeParameters, (predicateParameter, callParameter) => new PredicateParameterMapping(predicateParameter, callParameter))))
                            .Concat(invokeParameterBindingExpressions)
                            .Concat(predicate.Body)
                            .Append(new EndVariableLifeCyclePredicateExpression())
                            .Concat(parameterBindingExpressions)
                            .Concat(context.RemainingExpressions)
                    );
        }

        private static bool _Visit(PredicateProcessorContext context, BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression)
        {
            Debug.WriteLine("[begin variable life cycle]");
            Debug.Indent();

            context.BeginVariableLifeCycle(beginVariableLifeCycleExpression.ParameterMappings);
            return true;
        }

        private static bool _Visit(PredicateProcessorContext context, EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression)
        {
            Debug.Unindent();
            Debug.WriteLine("[end variable life cycle]");

            context.EndVariableLifeCycle();
            return true;
        }

        private static bool _Visit(PredicateProcessorContext context, CheckPredicateExpression checkExpression)
        {
            var result = checkExpression.Check(new PredicateExpressionContext(context.VariableLifeCycleContext));
            Debug.WriteLine("[check expression] " + (result ? "[continue]" : "[end]"));
            return result;
        }

        private static CallParameter _GetCallParameter(PredicateProcessorContext context, Parameter parameter)
        {
            var variableLifeCycleContext = context.VariableLifeCycleContext;
            if (parameter is PredicateParameter expressionPredicateParameter)
                return variableLifeCycleContext.CallParameterMapping[expressionPredicateParameter];
            else if (parameter is CallParameter expressionCallParameter)
            {
                variableLifeCycleContext.AddVariable(expressionCallParameter);
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
                    mapping => mapping.CallParameter.IsInput
                        ? mapping.PredicateParameter.ParameterType.IsAssignableFrom(mapping.CallParameter.ParameterType)
                        : mapping.CallParameter.IsOutput
                        ? mapping.CallParameter.ParameterType.IsAssignableFrom(mapping.PredicateParameter.ParameterType)
                        : false
                );
    }
}