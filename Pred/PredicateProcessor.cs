using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pred
{
    public class PredicateProcessor
    {
        private readonly IPredicateProvider _predicateProvider;

        public PredicateProcessor(IPredicateProvider predicateProvider)
            => _predicateProvider = predicateProvider;

        public PredicateProcessor(IEnumerable<Predicate> predicates)
            : this(new InMemoryPredicateProvider(predicates))
        {
        }

        public PredicateProcessor(params Predicate[] predicates)
            : this((IEnumerable<Predicate>)predicates)
        {
        }

        public Task<IEnumerable<object>> ProcessAsync(string predicateName)
            => ProcessAsync(predicateName, CancellationToken.None);

        public async Task<IEnumerable<object>> ProcessAsync(string predicateName, CancellationToken cancellationToken)
        {
            var predicates = _predicateProvider.GetPredicates(predicateName);
            var predicate = predicates.GetAsyncEnumerator(cancellationToken);
            try
            {
                await predicate.MoveNextAsync();
                return Enumerable.Empty<object>();
            }
            finally
            {
                await predicate.DisposeAsync();
            }
        }
    }
}