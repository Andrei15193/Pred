using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

        public IAsyncEnumerable<Predicate> GetPredicatesAsync(string predicateName)
            => GetPredicatesAsync(predicateName, CancellationToken.None);

        public async IAsyncEnumerable<Predicate> GetPredicatesAsync(string predicateName, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            if (_predicatesByName.Contains(predicateName))
                foreach (var predicate in _predicatesByName[predicateName])
                    yield return predicate;
        }
    }
}