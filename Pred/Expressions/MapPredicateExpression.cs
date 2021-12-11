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

    public class MapPredicateExpression<TParameter, TResult> : MapPredicateExpression
    {
        public MapPredicateExpression(ParameterPredicateExpression parameterExpression, Func<TParameter, TResult> selector)
            : base(typeof(TResult), parameterExpression, parameterValue => selector((TParameter)parameterValue))
        {
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public MapPredicateExpression(PredicateParameter<TParameter> parameter, Func<TParameter, TResult> selector)
            : this(parameter is object ? new ParameterPredicateExpression(parameter) : throw new ArgumentNullException(nameof(parameter)), selector)
        {
        }

        public MapPredicateExpression(InputParameter<TParameter> parameter, Func<TParameter, TResult> selector)
            : this(parameter is object ? new ParameterPredicateExpression(parameter) : throw new ArgumentNullException(nameof(parameter)), selector)
        {
        }

        public MapPredicateExpression(OutputParameter<TParameter> parameter, Func<TParameter, TResult> selector)
            : this(parameter is object ? new ParameterPredicateExpression(parameter) : throw new ArgumentNullException(nameof(parameter)), selector)
        {
        }

        public new Func<TParameter, TResult> Selector { get; }
    }
}