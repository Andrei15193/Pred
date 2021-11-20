using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public class Predicate
    {
        public Predicate(string name, IEnumerable<Parameter> parameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters as IReadOnlyList<Parameter> ?? parameters?.ToArray();
            if (Parameters is null || Parameters.Any(parameter => parameter is null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));
        }

        public Predicate(string name, params Parameter[] parameters)
            : this(name, (IEnumerable<Parameter>)parameters)
        {
        }

        public string Name { get; }

        public IReadOnlyList<Parameter> Parameters { get; }
    }
}