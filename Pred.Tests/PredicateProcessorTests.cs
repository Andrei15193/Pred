using System;
using System.Linq;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorTests
    {
        [Fact]
        public void Process_WhenPredicateExists_ReturnsEmptyCollection()
        {
            var predicateProcessor = new PredicateProcessor(new[] { new Predicate("MyPredicate") });

            var result = predicateProcessor.Process("myPredicate");

            Assert.Empty(result);
        }

        [Fact]
        public void Process_WhenPredicateDoesNotExist_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor(Enumerable.Empty<Predicate>());

            var exception = Assert.Throws<ArgumentException>("predicateName", () => predicateProcessor.Process("predicate that does not exist"));
            Assert.Equal(new ArgumentException("'predicate that does not exist' predicate does not exist.", "predicateName").Message, exception.Message);
        }
    }
}