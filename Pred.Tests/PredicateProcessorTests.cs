using System;
using System.Linq;
using System.Threading.Tasks;
using Pred.Expressions;
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

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 6)]
        [InlineData(4, 24)]
        public async Task ProcessAsync_Factorial(int input, int factorialResult)
        {
            var parameter1 = new PredicateParameter<int>("parameter1");
            var parameter2 = new PredicateParameter<int>("parameter2");

            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "factorial", new[] { parameter1, parameter2 },
                    parameters =>
                    {
                        var intermediary1 = new OutputParameter<int>("intermediary1");
                        var intermediary2 = new OutputParameter<int>("intermediary2");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.Check(context => context.Get<int>(parameters["parameter1"]).BoundValue > 1),
                            PredicateExpression.BindOrCheck(intermediary1, PredicateExpression.Map(context => context.Get<int>(parameters["parameter1"]).BoundValue - 1)),
                            PredicateExpression.Call("factorial", PredicateExpression.Parameter(intermediary1), PredicateExpression.Parameter(intermediary2)),
                            PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Map(context => context.Get<int>(parameters["parameter1"]).BoundValue * context.Get(intermediary2).BoundValue))
                        };
                    }
                ),
                new Predicate(
                    "factorial", new[] { parameter1, parameter2 },
                    PredicateExpression.Check(context => context.Get(parameter1).BoundValue <= 1),
                    PredicateExpression.BindOrCheck(parameter2, PredicateExpression.Constant(1))
                )
            );

            var results = await predicateProcessor.ProcessAsync("factorial", new InputParameter<int>("input", input), new OutputParameter<int>("output")).ToListAsync();

            var result = Assert.Single(results);
            Assert.Equal(factorialResult, result.Get<int>("output").BoundValue);
        }
    }
}