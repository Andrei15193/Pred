using System;
using System.Collections.Generic;

namespace Pred
{
    internal class ProcessorPredicateProvider
    {
        private readonly Func<IAsyncEnumerable<Predicate>> _predicateProvider;

        internal ProcessorPredicateProvider(Func<IAsyncEnumerable<Predicate>> predicateProvider)
            => _predicateProvider = predicateProvider;

        internal ProcessorPredicateProvider(Func<IAsyncEnumerable<Predicate>> predicateProvider, PredicateProcessorContext baseContext)
            => (_predicateProvider, BaseContext) = (predicateProvider, baseContext);

        internal PredicateProcessorContext BaseContext { get; }

        public IAsyncEnumerable<Predicate> GetPredicatesAsync()
            => _predicateProvider();
    }
}