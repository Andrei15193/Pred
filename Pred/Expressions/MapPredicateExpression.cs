using System;

namespace Pred.Expressions
{
    public class MapPredicateExpression : ValuePredicateExpression
    {
        internal MapPredicateExpression(Type valueType, Func<PredicateExpressionContext, object> selector)
            : base(valueType)
            => Selector = selector ?? throw new ArgumentNullException(nameof(selector));

        public Func<PredicateExpressionContext, object> Selector { get; }

        public sealed override void Accept(PredicateExpressionVisitor visitor)
            => visitor.VisitMapExpression(this);
    }

    public sealed class MapPredicateExpression<TResult> : MapPredicateExpression
    {
        internal MapPredicateExpression(Func<PredicateExpressionContext, TResult> selector)
            : base(typeof(TResult), context => selector(context))
            => Selector = selector ?? throw new ArgumentNullException(nameof(selector));

        public new Func<PredicateExpressionContext, TResult> Selector { get; }
    }
}