using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pred.Tests
{
    public static class Extensions
    {
        public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
            => source.ToListAsync(CancellationToken.None);

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            var result = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken))
                result.Add(item);
            return result;
        }
    }
}