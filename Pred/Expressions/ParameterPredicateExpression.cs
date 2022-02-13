using System;

namespace Pred.Expressions
{
    public sealed class ParameterPredicateExpression : ValuePredicateExpression
    {
        internal ParameterPredicateExpression(Parameter parameter)
            : base(parameter is null ? throw new ArgumentNullException(nameof(parameter)) : parameter.ParameterType)
            => Parameter = parameter;

        public new Parameter Parameter { get; }

        public sealed override void Accept(PredicateExpressionVisitor visitor)
            => visitor.VisitParameterExpression(this);
    }
}