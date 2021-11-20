using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public class PredicateProcessor
    {
        private readonly ILookup<string, Predicate> _predicatesByName;

        public PredicateProcessor(IEnumerable<Predicate> predicates)
            => _predicatesByName = predicates?.ToLookup(predicate => predicate.Name, StringComparer.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(predicates));

        public IEnumerable<object> Process(string predicateName)
        {
            if (!_predicatesByName.Contains(predicateName))
                throw new ArgumentException($"'{predicateName}' predicate does not exist.", nameof(predicateName));

            return Enumerable.Empty<object>();
        }
    }
}