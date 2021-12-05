using System;
using System.Collections.Generic;
using System.Threading;

namespace Pred
{
    internal class ProcessorPredicateProvider
    {
        private readonly Func<CancellationToken, IAsyncEnumerable<Predicate>> _predicateProvider;

        internal ProcessorPredicateProvider(Func<CancellationToken, IAsyncEnumerable<Predicate>> predicateProvider)
            => _predicateProvider = predicateProvider;

        public IAsyncEnumerable<Predicate> GetPredicatesAsync(CancellationToken cancellationToken)
            => _predicateProvider(cancellationToken);
    }
}