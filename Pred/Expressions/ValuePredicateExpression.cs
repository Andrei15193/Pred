using System;

namespace Pred.Expressions
{
    public abstract class ValuePredicateExpression : PredicateExpression
    {
        internal ValuePredicateExpression(Type valueType)
            => ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));

        public Type ValueType { get; }
    }
}