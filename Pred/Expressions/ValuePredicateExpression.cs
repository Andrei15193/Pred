using System;

namespace Pred.Expressions
{
    public class ValuePredicateExpression : PredicateExpression
    {
        internal ValuePredicateExpression(object value, Type valueType)
        {
            Value = value;
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        public object Value { get; }

        public Type ValueType { get; }
    }

    public class ValuePredicateExpression<T> : ValuePredicateExpression
    {
        public ValuePredicateExpression(T value)
            : base(value, typeof(T))
            => Value = value;

        public new T Value { get; }
    }
}