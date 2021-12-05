using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred.Expressions
{
    internal sealed class BeginVariableLifeCyclePredicateExpression : PredicateExpression
    {
        public BeginVariableLifeCyclePredicateExpression(IEnumerable<PredicateParameterMapping> parameterMappings)
        {
            ParameterMappings = parameterMappings as IReadOnlyList<PredicateParameterMapping> ?? parameterMappings?.ToArray();

            if (ParameterMappings is null || ParameterMappings.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameter mappings.", nameof(parameterMappings));
        }

        public IReadOnlyList<PredicateParameterMapping> ParameterMappings { get; }
    }
}