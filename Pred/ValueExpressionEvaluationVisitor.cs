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

        internal protected override void VisitConstantExpression(ConstantPredicateExpression constantExpression)
        {
            Debug.Write($"([constant] {_GetDebugValue(constantExpression.Value)})");
            Result = constantExpression.Value;
        }

        internal protected override void VisitBindOrCheckExpression(BindOrCheckPredicateExpression bindOrCheckExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(BindOrCheckPredicateExpression)}'.");

        internal protected override void VisitParameterExpression(ParameterPredicateExpression parameterExpression)
        {
            var callParameter = _context.VariableLifeCycleContext.GetOrAddCallParameter(parameterExpression.Parameter, out var resultParameter);
            Debug.Write($"([{(parameterExpression.Parameter is PredicateParameter predicateParameter ? $"predicate parameter: {predicateParameter.Name}, " : "")}call parameter: {callParameter.Name}] {_GetDebugValue(resultParameter.BoundValue)})");
            Result = resultParameter.BoundValue;
        }

        internal protected override void VisitCallExpression(CallPredicateExpression callExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(CallPredicateExpression)}'.");

        internal protected override void VisitMapExpression(MapPredicateExpression mapExpression)
        {
            Debug.Write("([begin map expression] ");
            Result = mapExpression.Selector(new PredicateExpressionContext(_context.VariableLifeCycleContext));
            Debug.Write(" [end map expression]");
            Debug.Write(_GetDebugValue(Result));
            Debug.Write(")");
        }

        internal protected override void VisitCheckExpression(CheckPredicateExpression checkExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(CheckPredicateExpression)}'.");

        internal protected override void VisitActionExpression(ActionPredicateExpression actionExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(ActionPredicateExpression)}'.");

        internal override void VisitBeginVariableLifeCycleExpression(BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(BeginVariableLifeCyclePredicateExpression)}'.");

        internal override void VisitEndVariableLifeCycleExpression(EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression)
            => throw new NotImplementedException($"Unhandled expression type '{typeof(EndVariableLifeCyclePredicateExpression)}'.");

        private static string _GetDebugValue(object value)
        {
            switch (value)
            {
                case string @string:
                    return $"\"{@string.Replace("\"", "\\\"")}\"";

                case IEnumerable collection:
                    return $"{{ {string.Join(", ", collection.OfType<object>().Select(_GetDebugValue))} }}";

                case null:
                    return "null";

                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }
    }
}