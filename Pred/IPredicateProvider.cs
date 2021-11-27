using System.Collections.Generic;
using System.Threading;

namespace Pred
{
    public interface IPredicateProvider
    {
        IAsyncEnumerable<Predicate> GetPredicatesAsync(string name);

        IAsyncEnumerable<Predicate> GetPredicatesAsync(string name, CancellationToken cancellationToken);
    }
}