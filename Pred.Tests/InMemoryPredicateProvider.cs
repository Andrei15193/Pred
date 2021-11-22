using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact]
        public async Task GetPredicates_WhenPredicateDoesNotExist_ReturnsEmptyCollection()
        {
            var predicateProvider = new InMemoryPredicateProvider();

            var result = predicateProvider.GetPredicates("predicate that does not exist");

            var predicates = new List<Predicate>();
            await foreach (var predicate in result)
                predicates.Add(predicate);
            Assert.Empty(predicates);
        }

        [Fact]
        public async Task GetPredicates_WhenPredicatesMatch_ReturnsOnlyOnesHavingSameName()
        {
            var predicate1s = new[]
            {
                new Predicate("predicate1"),
                new Predicate("predicate1", new PredicateParameter<int>("parameter1")),
                new Predicate("predicate1", new PredicateParameter<int>("parameter1"), new PredicateParameter<int>("parameter2"))
            };
            var predicateProvider = new InMemoryPredicateProvider(predicate1s.Append(new Predicate("predicate2")));

            var result = predicateProvider.GetPredicates("predicate1");

            var predicates = new List<Predicate>();
            await foreach (var predicate in result)
                predicates.Add(predicate);
            Assert.Equal(predicate1s, predicates);
        }
    }
}