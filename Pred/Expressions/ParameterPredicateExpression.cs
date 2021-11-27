using System;

namespace Pred.Expressions
{
    public sealed class ParameterPredicateExpression : ValuePredicateExpression
    {
        public ParameterPredicateExpression(Parameter parameter)
            : base(parameter is null ? throw new ArgumentNullException(nameof(parameter)) : parameter.ParameterType)
            => Parameter = parameter;

        public Parameter Parameter { get; }
    }
}