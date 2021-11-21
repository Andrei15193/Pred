using System;

namespace Pred.Expressions
{
    public class BindOrCheckPredicateExpression : PredicateExpression
    {
        public BindOrCheckPredicateExpression(Parameter parameter, ValuePredicateExpression value)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Value = value ?? throw new ArgumentNullException(nameof(value));

            if (!Parameter.ParameterType.IsAssignableFrom(Value.ValueType))
                throw new ArgumentException($"Cannot assign value of type '{value.ValueType}' (value) to '{Parameter.ParameterType}' (parameter).", nameof(value));
        }

        public Parameter Parameter { get; }

        public ValuePredicateExpression Value { get; }
    }
}