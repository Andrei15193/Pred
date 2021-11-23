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
                new Predicate("MyPredicate1", new PredicateParameter<int>("parameter1"), new PredicateParameter<int>("parameter2")),
                new Predicate("MyPredicate1", new PredicateParameter<int>("parameter1"), new PredicateParameter<object>("parameter2")),
                new Predicate("MyPredicate1", new PredicateParameter<object>("parameter1"), new PredicateParameter<int>("parameter2")),
                new Predicate("MyPredicate1", new PredicateParameter<object>("parameter1"), new PredicateParameter<object>("parameter2")),
                new Predicate("myPredicate1"),
                new Predicate("myPredicate1", new PredicateParameter<int>("parameter1"), new PredicateParameter<int>("parameter2")),
                new Predicate("myPredicate1", new PredicateParameter<int>("parameter1"), new PredicateParameter<object>("parameter2")),
                new Predicate("myPredicate1", new PredicateParameter<object>("parameter1"), new PredicateParameter<int>("parameter2")),
                new Predicate("myPredicate1", new PredicateParameter<object>("parameter1"), new PredicateParameter<object>("parameter2")),
                new Predicate("MyPredicate2")
            );

            var callParameter1 = Parameter.Input<object>("parameter1", 10);
            var callParameter2 = Parameter.Output<int>("parameter2");
            var results = await predicateProcessor.ProcessAsync("MyPredicate1", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);

            var resultParameter1 = Assert.IsType<ResultParameter<object>>(result["parameter1"]);
            Assert.True(resultParameter1.IsBoundToValue);
            Assert.Equal(new[] { callParameter1 }, resultParameter1.BoundParameters);
            Assert.Equal(typeof(object), resultParameter1.ParameterType);
            Assert.Equal(10, resultParameter1.BoundValue);

            var resultParameter2 = Assert.IsType<ResultParameter<int>>(result["parameter2"]);
            Assert.False(resultParameter2.IsBoundToValue);
            Assert.Equal(new[] { callParameter2 }, resultParameter2.BoundParameters);
            Assert.Equal(typeof(int), resultParameter2.ParameterType);
            var exception = Assert.Throws<InvalidOperationException>(() => resultParameter2.BoundValue);
            Assert.Equal(new InvalidOperationException("The parameter is not bound to a value.").Message, exception.Message);
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