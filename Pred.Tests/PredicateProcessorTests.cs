using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorTests
    {
        [Fact]
        public void Process_WhenPredicateNameIsNull_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentNullException>("predicateName", () => predicateProcessor.ProcessAsync(null));
            Assert.Equal(new ArgumentNullException("predicateName").Message, exception.Message);
        }

        [Fact]
        public void Process_WhenParametersAreNull_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentException>("parameters", () => predicateProcessor.ProcessAsync("", default(CallParameter[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Process_WhenPassingNullParameter_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentException>("parameters", () => predicateProcessor.ProcessAsync("", default(CallParameter)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public async Task Process_WhenPredicateExists_ReturnsCollectionAsManyItemsAsMatchingPredicates()
        {
            var predicates = new[]
            {
                new Predicate("MyPredicate1"),
                new Predicate("MyPredicate1", new Parameter<int>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<int>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<object>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<object>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("myPredicate1"),
                new Predicate("MyPredicate2")
            };
            var predicateProcessor = new PredicateProcessor(predicates);

            var results = predicateProcessor.ProcessAsync("MyPredicate1", new[] { Parameter.Input<object>(10), Parameter.Output<int>("parameter2") });

            var allResults = new List<object>();
            await foreach (var result in results)
                allResults.Add(result);
            Assert.Same(predicates[3], Assert.Single(allResults));
        }

        [Fact]
        public async Task Process_WhenPredicateDoesNotExist_ReturnsEmptyResult()
        {
            var predicateProcessor = new PredicateProcessor(Enumerable.Empty<Predicate>());

            var results = predicateProcessor.ProcessAsync("predicate that does not exist");

            var allResults = new List<object>();
            await foreach (var result in results)
                allResults.Add(result);
            Assert.Empty(allResults);
        }
    }
}