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
    internal sealed class PredicateProcessorVisitor : PredicateExpressionVisitor
    {
        private readonly PredicateProcessorContext _context;

        internal PredicateProcessorVisitor(PredicateProcessorContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        public bool IsExpressionTrue { get; private set; } = true;

        internal protected override void Visit(ConstantPredicateExpression constantExpression)
            => IsExpressionTrue = !_IsFalsy(_Evaluate(constantExpression));

        internal protected override void Visit(BindOrCheckPredicateExpression bindOrCheckExpression)
        {
            var callParameter = _context.VariableLifeCycleContext.GetOrAddCallParameter(bindOrCheckExpression.Parameter, out var resultParameter);

            switch (bindOrCheckExpression.Value)
            {
                case ParameterPredicateExpression parameterExpression:
                    var otherCallParameter = _context.VariableLifeCycleContext.GetOrAddCallParameter(parameterExpression.Parameter, out var otherResultParameter);

                    Debug.WriteLine($"{callParameter.Name} = {otherCallParameter.Name}");

                    if (resultParameter.IsBoundToValue && otherResultParameter.IsBoundToValue)
                        IsExpressionTrue = Equals(resultParameter.BoundValue, otherResultParameter.BoundValue);
                    else
                    {
                        resultParameter.BindParameter(otherResultParameter);
                        _context.VariableLifeCycleContext.ResultParameterMapping[otherCallParameter] = resultParameter;

                        if (otherResultParameter.IsBoundToValue)
                            resultParameter.BindValue(otherResultParameter.BoundValue);
                    }
                    break;

                case ValuePredicateExpression valueExpression:
                    Debug.Write($"{callParameter.Name} = ");
                    var value = _Evaluate(valueExpression);
                    Debug.WriteLine("");

                    if (resultParameter.IsBoundToValue)
                        IsExpressionTrue = Equals(resultParameter.BoundValue, value);
                    else
                        resultParameter.BindValue(value);
                    break;

                default:
                    throw new NotImplementedException($"Unhandled expression type '{bindOrCheckExpression.Value.GetType()}'.");
            }
        }

        internal protected override void Visit(ParameterPredicateExpression parameterExpression)
        {
            _context.VariableLifeCycleContext.GetOrAddCallParameter(parameterExpression.Parameter, out var resultParameter);
            if (resultParameter.IsBoundToValue)
                IsExpressionTrue = !_IsFalsy(resultParameter.BoundValue);
            else
                IsExpressionTrue = false;
        }

        internal protected override void Visit(CallPredicateExpression callExpression)
        {
            var invokeParameters = callExpression
                .Parameters
                .Select((parameter, parameterIndex) =>
                {
                    if (parameter is ParameterPredicateExpression parameterExpression)
                        return _context.VariableLifeCycleContext.GetOrAddCallParameter(parameterExpression.Parameter);
                    else if (parameter is ConstantPredicateExpression constantExpression)
                        return Parameter.Input(constantExpression.ValueType, constantExpression.Value);
                    else
                        throw new InvalidOperationException($"Unhandled expression type '{parameter.GetType()}'.");
                })
                .ToArray();

            Debug.WriteLine($"{callExpression.Name}({string.Join(", ", invokeParameters.Select(callParameter => callParameter.Name ?? ((InputParameter)callParameter).Value))})");

            _context.AddPredicateProvider(cancellationToken => _ProcessCallAsync(callExpression.Name, invokeParameters, cancellationToken));
            IsExpressionTrue = false;
        }

        internal protected override void Visit(MapPredicateExpression mapExpression)
            => IsExpressionTrue = !_IsFalsy(_Evaluate(mapExpression));

        internal protected override void Visit(CheckPredicateExpression checkExpression)
        {
            Debug.WriteLine("[begin check expression]");
            Debug.Indent();

            IsExpressionTrue = checkExpression.Check(new PredicateExpressionContext(_context.VariableLifeCycleContext));

            Debug.Unindent();
            Debug.WriteLine("[end check expression]");
            Debug.WriteLine(IsExpressionTrue ? "[continue]" : "[end]");
        }

        internal protected override void Visit(ActionPredicateExpression actionExpression)
        {
            Debug.WriteLine("[begin action expression]");
            Debug.Indent();

            actionExpression.Process(new PredicateExpressionContext(_context.VariableLifeCycleContext));

            Debug.Unindent();
            Debug.WriteLine("[end action expression]");
        }

        internal override void Visit(BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression)
        {
            Debug.WriteLine("[begin variable life cycle]");
            Debug.Indent();

            _context.BeginVariableLifeCycle(beginVariableLifeCycleExpression.ParameterMappings);
        }

        internal override void Visit(EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression)
        {
            Debug.Unindent();
            Debug.WriteLine("[end variable life cycle]");

            _context.EndVariableLifeCycle();
        }

        private object _Evaluate(ValuePredicateExpression valueExpression)
        {
            var visitor = new ValueExpressionEvaluationVisitor(_context);
            valueExpression.Accept(visitor);
            return visitor.Result;
        }

        private async IAsyncEnumerable<Predicate> _ProcessCallAsync(string predicateName, IReadOnlyList<CallParameter> invokeParameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var variableLifeCycleContext = _context.VariableLifeCycleContext;

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

            await foreach (var predicate in _context.PredicateProvider.GetPredicatesAsync(predicateName, cancellationToken).WithCancellation(cancellationToken))
                if (CallParameter.AreParametersMatching(invokeParameters, predicate.Parameters))
                    yield return new Predicate(
                        _context.Predicate.Parameters,
                        Enumerable
                            .Empty<PredicateExpression>()
                            .Append(new BeginVariableLifeCyclePredicateExpression(predicate.Parameters.Zip(invokeParameters, (predicateParameter, callParameter) => new PredicateParameterMapping(predicateParameter, callParameter))))
                            .Concat(invokeParameterBindingExpressions)
                            .Concat(predicate.Body)
                            .Append(new EndVariableLifeCyclePredicateExpression())
                            .Concat(parameterBindingExpressions)
                            .Concat(_context.RemainingExpressions)
                    );
        }

        private bool _IsFalsy(object value)
            => Equals(value, false) || Equals(value, null);
    }
}