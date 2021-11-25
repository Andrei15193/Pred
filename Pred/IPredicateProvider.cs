using System.Collections.Generic;

namespace Pred
{
    public interface IPredicateProvider
    {
        IAsyncEnumerable<Predicate> GetPredicatesAsync(string name);
    }
}