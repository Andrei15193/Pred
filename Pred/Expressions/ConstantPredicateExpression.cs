using System;

namespace Pred.Expressions
{
    public class ConstantPredicateExpression : ValuePredicateExpression
    {
        internal ConstantPredicateExpression(object value, Type valueType)
            : base(valueType)
            => Value = value;

        public object Value { get; }
    }

    public sealed class ConstantPredicateExpression<T> : ConstantPredicateExpression
    {
        public static implicit operator ConstantPredicateExpression<T>(T value)
            => new ConstantPredicateExpression<T>(value);

        public ConstantPredicateExpression(T value)
            : base(value, typeof(T))
            => Value = value;

        public new T Value { get; }
    }
}