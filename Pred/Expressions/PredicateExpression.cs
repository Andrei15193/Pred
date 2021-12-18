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

        public static MapPredicateExpression<TResult> Map<TResult>(Func<PredicateExpressionContext, TResult> selector)
            => new MapPredicateExpression<TResult>(selector);

        public static CheckPredicateExpression Check(Func<PredicateExpressionContext, bool> callback)
            => new CallbackCheckPredicateExpression(callback);

        public static ActionPredicateExpression Action(Action<PredicateExpressionContext> callback)
            => new CallbackActionPredicateExpression(callback);

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

        internal sealed class CallbackActionPredicateExpression : ActionPredicateExpression
        {
            private readonly Action<PredicateExpressionContext> _callback;

            public CallbackActionPredicateExpression(Action<PredicateExpressionContext> callback)
                => _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            protected internal override void Process(PredicateExpressionContext context)
                => _callback(context);
        }
    }
}