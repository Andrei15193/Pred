using System;

namespace Pred.Expressions
{
    public class BindOrCheckPredicateExpression : PredicateExpression
    {
        public BindOrCheckPredicateExpression(PredicateParameter parameter, ConstantPredicateExpression constantExpression)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Value = constantExpression ?? throw new ArgumentNullException(nameof(constantExpression));

            if (!Parameter.ParameterType.IsAssignableFrom(Value.ValueType))
                throw new ArgumentException($"Cannot assign value of type '{constantExpression.ValueType}' (value) to '{Parameter.ParameterType}' (parameter).", nameof(constantExpression));
        }

        public BindOrCheckPredicateExpression(PredicateParameter parameter, ParameterPredicateExpression parameterExpression)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Value = parameterExpression ?? throw new ArgumentNullException(nameof(parameterExpression));

            if (Parameter.ParameterType != Value.ValueType)
                throw new ArgumentException($"Cannot assign value of type '{parameterExpression.ValueType}' (value) to '{Parameter.ParameterType}' (parameter).", nameof(parameterExpression));
        }

        public PredicateParameter Parameter { get; }

        public ValuePredicateExpression Value { get; }
    }
}