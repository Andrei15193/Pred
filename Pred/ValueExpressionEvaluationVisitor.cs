using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Pred.Expressions;

namespace Pred
{
    internal sealed class ValueExpressionEvaluationVisitor : PredicateExpressionVisitor
    {
        private readonly PredicateProcessorContext _context;

        internal ValueExpressionEvaluationVisitor(PredicateProcessorContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        internal object Result { get; set; }

        internal protected override void Visit(ConstantPredicateExpression constantExpression)
        {
            Debug.Write($"([constant] {_GetValue(constantExpression.Value)})");
            Result = constantExpression.Value;
        }

        internal protected override void Visit(BindOrCheckPredicateExpression bindOrCheckExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(BindOrCheckPredicateExpression)}'.");

        internal protected override void Visit(ParameterPredicateExpression parameterExpression)
        {
            var callParameter = _context.VariableLifeCycleContext.GetOrAddCallParameter(parameterExpression.Parameter, out var resultParameter);
            Debug.Write($"([{(parameterExpression.Parameter is PredicateParameter predicateParameter ? $"predicate parameter: {predicateParameter.Name}, " : "")}call parameter: {callParameter.Name}] {_GetValue(resultParameter.BoundValue)})");
            Result = resultParameter.BoundValue;
        }

        internal protected override void Visit(CallPredicateExpression callExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(CallPredicateExpression)}'.");

        internal protected override void Visit(MapPredicateExpression mapExpression)
        {
            Debug.Write("[begin map expression] ");
            Result = mapExpression.Selector(new PredicateExpressionContext(_context.VariableLifeCycleContext));
            Debug.Write(_GetValue(Result));
            Debug.Write(" [end map expression]");
        }

        internal protected override void Visit(CheckPredicateExpression checkExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(CheckPredicateExpression)}'.");

        internal protected override void Visit(ActionPredicateExpression actionExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(ActionPredicateExpression)}'.");

        internal override void Visit(BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(BeginVariableLifeCyclePredicateExpression)}'.");

        internal override void Visit(EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(EndVariableLifeCyclePredicateExpression)}'.");

        private static string _GetValue(object value)
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
}