using System;
using System.Collections.Generic;
using System.Linq;
using Pred.Expressions;

namespace Pred
{
    public class Predicate
    {
        public Predicate(string name, IEnumerable<PredicateParameter> parameters, IEnumerable<PredicateExpression> body)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters as IReadOnlyList<PredicateParameter> ?? parameters?.ToArray();
            if (Parameters is null || Parameters.Any(parameter => parameter is null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            Body = body as IReadOnlyList<PredicateExpression> ?? body?.ToArray();
            if (Body is null || Body.Any(expression => expression is null))
                throw new ArgumentException("Cannot be null or contain null expressions.", nameof(body));
        }

        public Predicate(string name, params PredicateParameter[] parameters)
            : this(name, (IEnumerable<PredicateParameter>)parameters, Enumerable.Empty<PredicateExpression>())
        {
        }

        public Predicate(string name, IEnumerable<PredicateParameter> parameters, params PredicateExpression[] body)
            : this(name, parameters, (IEnumerable<PredicateExpression>)body)
        {
        }

        public Predicate(string name, IEnumerable<PredicateParameter> parameters, Func<IReadOnlyDictionary<string, PredicateParameter>, IEnumerable<PredicateExpression>> bodyProvider)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters as IReadOnlyList<PredicateParameter> ?? parameters?.ToArray();
            if (Parameters is null || Parameters.Any(parameter => parameter is null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            var body = bodyProvider is null ? null : bodyProvider(Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal));
            Body = body as IReadOnlyList<PredicateExpression> ?? body?.ToArray();
            if (Body is null || Body.Any(expression => expression is null))
                throw new ArgumentException("Cannot be null, return null or return expressions contain null.", nameof(bodyProvider));
        }

        public string Name { get; }

        public IReadOnlyList<PredicateParameter> Parameters { get; }

        public IReadOnlyList<PredicateExpression> Body { get; }
    }
}