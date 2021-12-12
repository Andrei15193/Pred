using System.Linq;
using System.Threading.Tasks;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorCallTests
    {
        [Fact]
        public async Task ProcessAsync_WithPredicateCall_BindsParameters()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(parameters["parameter"]), PredicateExpression.Constant<int>(20))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Parameter(parameters["parameter2"]))
                    }
                )
            );

            var callParameter = Parameter.Output<object>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result[callParameter], Assert.Single(result));
            Assert.Equal(typeof(object), result[0].ParameterType);
            Assert.True(result["output"].IsBoundToValue);
            Assert.Equal(20, result.Get<object>(callParameter).BoundValue);
            Assert.Equal(new[] { callParameter }, result.GetAt<object>(0).BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_WithPredicateCallMatchingMultiplePredicates_ReturnsAllResults()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(parameters["parameter"]), PredicateExpression.Constant<int>(30))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(20))
                    }
                )
            );

            var callParameter = Parameter.Output<object>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Equal(2, results.Count);
            foreach (var (value, result) in new[] { 10, 20 }.Zip(results, (value, result) => (value, result)))
            {
                Assert.Same(result[callParameter], Assert.Single(result));
                Assert.Equal(typeof(object), result[0].ParameterType);
                Assert.True(result["output"].IsBoundToValue);
                Assert.Equal(value, result.Get<object>(callParameter).BoundValue);
                Assert.Equal(new[] { callParameter }, result.GetAt<object>(0).BoundParameters);
            }
        }

        [Fact]
        public async Task ProcessAsync_WithPredicateCallMatchingMultiplePredicatesAndCheckAfterwards_ReturnsMatchingResults()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(parameters["parameter"]), PredicateExpression.Constant<int>(30)),
                        PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(20))
                    }
                )
            );

            var callParameter = Parameter.Output<object>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result[callParameter], Assert.Single(result));
            Assert.Equal(typeof(object), result[0].ParameterType);
            Assert.True(result["output"].IsBoundToValue);
            Assert.Equal(10, result.Get<object>(callParameter).BoundValue);
            Assert.Equal(new[] { callParameter }, result.GetAt<object>(0).BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_WithOutputParameterPredicateCall_ReturnsAllResults()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters =>
                    {
                        var outputParameter = Parameter.Output<int>("outputParameter");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(outputParameter)),
                            PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Parameter(outputParameter))
                        };
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Constant<int>(20))
                    }
                )
            );

            var callParameter = Parameter.Output<int>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Equal(2, results.Count);
            foreach (var (value, result) in new[] { 10, 20 }.Zip(results, (value, result) => (value, result)))
            {
                Assert.Same(result[callParameter], Assert.Single(result));
                Assert.Equal(typeof(int), result[0].ParameterType);
                Assert.True(result["output"].IsBoundToValue);
                Assert.Equal(value, result.Get<int>(callParameter).BoundValue);
                Assert.Equal(new[] { callParameter }, result.GetAt<int>(0).BoundParameters);
            }
        }

        [Fact]
        public async Task ProcessAsync_WithSameOutputParameterProvidedTwiceAndBindingAfterCall_ReturnsAllResults()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters =>
                    {
                        var outputParameter = Parameter.Output<int>("outputParameter");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(outputParameter), PredicateExpression.Parameter(outputParameter)),
                            PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Parameter(outputParameter))
                        };
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Parameter(parameters["parameter2"])),
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Parameter(parameters["parameter2"])),
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(20))
                    }
                )
            );

            var callParameter = Parameter.Output<int>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Equal(2, results.Count);
            foreach (var (value, result) in new[] { 10, 20 }.Zip(results, (value, result) => (value, result)))
            {
                Assert.Same(result[callParameter], Assert.Single(result));
                Assert.Equal(typeof(int), result[0].ParameterType);
                Assert.True(result["output"].IsBoundToValue);
                Assert.Equal(value, result.Get<int>(callParameter).BoundValue);
                Assert.Equal(new[] { callParameter }, result.GetAt<int>(0).BoundParameters);
            }
        }

        [Fact]
        public async Task ProcessAsync_WithSameOutputParameterProvidedTwiceAndBindingBeforeCall_ReturnsAllResults()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters =>
                    {
                        var outputParameter = Parameter.Output<int>("outputParameter");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Parameter(outputParameter)),
                            PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(outputParameter), PredicateExpression.Parameter(outputParameter)),
                        };
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Parameter(parameters["parameter2"])),
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(10))
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Parameter(parameters["parameter2"])),
                        PredicateExpression.BindOrCheck(parameters["parameter1"], PredicateExpression.Constant<int>(20))
                    }
                )
            );

            var callParameter = Parameter.Output<int>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Equal(2, results.Count);
            foreach (var (value, result) in new[] { 10, 20 }.Zip(results, (value, result) => (value, result)))
            {
                Assert.Same(result[callParameter], Assert.Single(result));
                Assert.Equal(typeof(int), result[0].ParameterType);
                Assert.True(result["output"].IsBoundToValue);
                Assert.Equal(value, result.Get<int>(callParameter).BoundValue);
                Assert.Equal(new[] { callParameter }, result.GetAt<int>(0).BoundParameters);
            }
        }
    }
}