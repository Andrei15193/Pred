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

        internal ProcessorPredicateProvider(Func<CancellationToken, IAsyncEnumerable<Predicate>> predicateProvider, PredicateProcessorContext baseContext)
            => (_predicateProvider, BaseContext) = (predicateProvider, baseContext);

        internal PredicateProcessorContext BaseContext { get; }

        public IAsyncEnumerable<Predicate> GetPredicatesAsync(CancellationToken cancellationToken)
            => _predicateProvider(cancellationToken);
    }
}