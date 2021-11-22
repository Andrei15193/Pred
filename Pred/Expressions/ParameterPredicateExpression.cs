using System;

namespace Pred.Expressions
{
    public sealed class ParameterPredicateExpression : ValuePredicateExpression
    {
        public ParameterPredicateExpression(PredicateParameter parameter)
            : base(parameter is null ? throw new ArgumentNullException(nameof(parameter)) : parameter.ParameterType)
            => Parameter = parameter;

        public PredicateParameter Parameter { get; }
    }
}