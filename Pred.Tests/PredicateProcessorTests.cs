using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorTests
    {
        [Fact]
        public void ProcessAsync_WhenPredicateNameIsNull_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentNullException>("predicateName", () => predicateProcessor.ProcessAsync(null));
            Assert.Equal(new ArgumentNullException("predicateName").Message, exception.Message);
        }

        [Fact]
        public void ProcessAsync_WhenParametersAreNull_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentException>("parameters", () => predicateProcessor.ProcessAsync("", default(CallParameter[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void ProcessAsync_WhenPassingNullParameter_ThrowsException()
        {
            var predicateProcessor = new PredicateProcessor();

            var exception = Assert.Throws<ArgumentException>("parameters", () => predicateProcessor.ProcessAsync("", default(CallParameter)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public async Task ProcessAsync_WhenPredicateExists_ReturnsCollectionAsManyItemsAsMatchingPredicates()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate("MyPredicate1"),
                new Predicate("MyPredicate1", new Parameter<int>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<int>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<object>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("MyPredicate1", new Parameter<object>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("myPredicate1"),
                new Predicate("myPredicate1", new Parameter<int>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("myPredicate1", new Parameter<int>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("myPredicate1", new Parameter<object>("parameter1"), new Parameter<int>("parameter2")),
                new Predicate("myPredicate1", new Parameter<object>("parameter1"), new Parameter<object>("parameter2")),
                new Predicate("MyPredicate2")
            );

            var results = await predicateProcessor.ProcessAsync("MyPredicate1", new[] { Parameter.Input<object>("parameter1", 10), Parameter.Output<int>("parameter2") }).ToListAsync();

            var result = Assert.Single(results);

            var parameter1 = Assert.IsType<PredicateProcessResultParameter<object>>(result["parameter1"]);
            Assert.Equal("parameter1", parameter1.Name);
            Assert.True(parameter1.IsBound);
            Assert.Equal(typeof(object), parameter1.ParameterType);
            Assert.Equal(10, parameter1.Value);

            var parameter2 = Assert.IsType<PredicateProcessResultParameter<int>>(result["parameter2"]);
            Assert.Equal("parameter2", parameter2.Name);
            Assert.False(parameter2.IsBound);
            Assert.Equal(typeof(int), parameter2.ParameterType);
            var exception = Assert.Throws<InvalidOperationException>(() => parameter2.Value);
            Assert.Equal(new InvalidOperationException("The parameter is not bound.").Message, exception.Message);
        }

        [Fact]
        public async Task ProcessAsync_WhenPredicateDoesNotExist_ReturnsEmptyResult()
        {
            var predicateProcessor = new PredicateProcessor(Enumerable.Empty<Predicate>());

            var results = await predicateProcessor.ProcessAsync("predicate that does not exist").ToListAsync();

            Assert.Empty(results);
        }
    }
}