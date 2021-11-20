using System;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorTests
    {
        [Fact]
        public void Process_WhenPredicateDoesNotExist_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentException>(() =>  predicateProcessor.Process("predicate that does not exist"));
            Assert.Equal(new ArgumentException("'predicate that does not exist' predicate does not exist.", "predicateName").Message, exception.Message);
        }
    }
}