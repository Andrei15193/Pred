using System;
using Xunit;

namespace Pred.Tests
{
    public class InMemoryPredicateProviderTests
    {
        [Fact]
        public void Create_WithNullPredicate_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("predicates", () => new InMemoryPredicateProvider(default(Predicate)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null predicates.", "predicates").Message, exception.Message);
        }
        
        [Fact]
        public void Create_WithNullPredicates_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("predicates", () => new InMemoryPredicateProvider(default(Predicate[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null predicates.", "predicates").Message, exception.Message);
        }
    }
}