using System;
using System.Collections.Generic;

namespace Pred.Expressions
{
    public abstract class PredicateExpression
    {
        public static ConstantPredicateExpression<T> Constant<T>(T value)
            => new ConstantPredicateExpression<T>(value);

        public static ParameterPredicateExpression Parameter(Parameter parameter)
            => new ParameterPredicateExpression(parameter);

        public static BindOrCheckPredicateExpression BindOrCheck(Parameter parameter, ValuePredicateExpression value)
            => new BindOrCheckPredicateExpression(parameter, value);

        public static CallPredicateExpression Call(string name, IEnumerable<ValuePredicateExpression> parameters)
            => new CallPredicateExpression(name, parameters);

        public static CallPredicateExpression Call(string name, params ValuePredicateExpression[] parameters)
            => new CallPredicateExpression(name, parameters);

        public static MapPredicateExpression<T, TResult> Map<T, TResult>(ParameterPredicateExpression parameter, Func<T, TResult> selector)
            => new MapPredicateExpression<T, TResult>(parameter, selector);

        public static MapPredicateExpression<T, TResult> Map<T, TResult>(PredicateParameter<T> parameter, Func<T, TResult> selector)
            => Map(Parameter(parameter), selector);

        public static MapPredicateExpression<T, TResult> Map<T, TResult>(InputParameter<T> parameter, Func<T, TResult> selector)
            => Map(Parameter(parameter), selector);

        public static MapPredicateExpression<T, TResult> Map<T, TResult>(OutputParameter<T> parameter, Func<T, TResult> selector)
            => Map(Parameter(parameter), selector);

        public static CheckPredicateExpression Check(Func<PredicateExpressionContext, bool> callback)
            => new CallbackCheckPredicateExpression(callback);

        internal PredicateExpression()
        {
        }

        internal sealed class CallbackCheckPredicateExpression : CheckPredicateExpression
        {
            private readonly Func<PredicateExpressionContext, bool> _callback;

            public CallbackCheckPredicateExpression(Func<PredicateExpressionContext, bool> callback)
                => _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            protected internal override bool Check(PredicateExpressionContext context)
                => _callback(context);
        }
    }
}