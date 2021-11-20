using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pred
{
    public class InMemoryPredicateProvider : IPredicateProvider
    {
        private readonly ILookup<string, Predicate> _predicatesByName;

        public InMemoryPredicateProvider(IEnumerable<Predicate> predicates)
            => _predicatesByName = predicates
            ?.ToLookup(
                predicate => predicate?.Name ?? throw new ArgumentException("Cannot be null or contain null predicates.", nameof(predicates)),
                StringComparer.Ordinal
            ) ?? throw new ArgumentException("Cannot be null or contain null predicates.", nameof(predicates));

        public InMemoryPredicateProvider(params Predicate[] predicates)
            : this((IEnumerable<Predicate>)predicates)
        {
        }

        public async IAsyncEnumerable<Predicate> GetPredicates(string predicateName)
        {
            await Task.Yield();

            if (_predicatesByName.Contains(predicateName))
                foreach (var predicate in _predicatesByName[predicateName])
                    yield return predicate;
            else
                throw new ArgumentException($"'{predicateName}' predicate does not exist.", nameof(predicateName));
        }
    }
}