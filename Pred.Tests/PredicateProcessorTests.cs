using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorTests
    {
        [Fact]
        public async Task Process_WhenPredicateExists_ReturnsEmptyCollection()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate("MyPredicate")
            );

            var result = await predicateProcessor.ProcessAsync("MyPredicate");

            Assert.Empty(result);
        }

        [Fact]
        public async Task Process_WhenPredicateDoesNotExist_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor(Enumerable.Empty<Predicate>());

            var exception = await Assert.ThrowsAsync<ArgumentException>("predicateName", () => predicateProcessor.ProcessAsync("predicate that does not exist"));
            Assert.Equal(new ArgumentException("'predicate that does not exist' predicate does not exist.", "predicateName").Message, exception.Message);
        }
    }
}