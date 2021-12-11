using System;

namespace Pred.Expressions
{
    public class BindOrCheckPredicateExpression : PredicateExpression
    {
        public BindOrCheckPredicateExpression(Parameter parameter, ValuePredicateExpression valueExpression)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Value = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));

            if (!Parameter.ParameterType.IsAssignableFrom(Value.ValueType))
                throw new ArgumentException($"Cannot assign value of type '{valueExpression.ValueType}' (value) to '{Parameter.ParameterType}' (parameter).", nameof(valueExpression));
        }

        public Parameter Parameter { get; }

        public ValuePredicateExpression Value { get; }
    }
}