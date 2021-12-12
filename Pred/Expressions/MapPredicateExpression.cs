using System;

namespace Pred.Expressions
{
    public class MapPredicateExpression : ValuePredicateExpression
    {
        internal MapPredicateExpression(Type valueType, ParameterPredicateExpression parameterExpression, Func<object, object> selector)
            : base(valueType)
        {
            ParameterExpression = parameterExpression ?? throw new ArgumentNullException(nameof(parameterExpression));
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public ParameterPredicateExpression ParameterExpression { get; }

        public Func<object, object> Selector { get; }
    }

    public sealed class MapPredicateExpression<TParameter, TResult> : MapPredicateExpression
    {
        internal MapPredicateExpression(ParameterPredicateExpression parameterExpression, Func<TParameter, TResult> selector)
            : base(typeof(TResult), parameterExpression, parameterValue => selector((TParameter)parameterValue))
            => Selector = selector ?? throw new ArgumentNullException(nameof(selector));

        public new Func<TParameter, TResult> Selector { get; }
    }
}