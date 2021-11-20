using System;
using System.Collections.Generic;

namespace Pred
{
    public interface IPredicateProvider
    {
        IAsyncEnumerable<Predicate> GetPredicates(string name);
    }
}