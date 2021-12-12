using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred.Expressions
{
    public sealed class CallPredicateExpression : PredicateExpression
    {
        internal CallPredicateExpression(string name, IEnumerable<ValuePredicateExpression> parameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters as IReadOnlyList<ValuePredicateExpression> ?? parameters?.ToArray();

            if (Parameters is null || Parameters.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));
        }

        public string Name { get; }

        public IReadOnlyList<ValuePredicateExpression> Parameters { get; }
    }
}