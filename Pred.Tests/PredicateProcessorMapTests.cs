using System;
using System.Threading.Tasks;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorMapTests
    {
        [Fact]
        public async Task ProcessAsync_WithMapExpression_TransformsParameter()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new PredicateParameter[] { Parameter.Predicate<string>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Map<string, object>((PredicateParameter<string>)parameters["parameter1"], x => x))
                    }
                )
            );

            var callParameter1 = Parameter.Input<string>("input", "value");
            var callParameter2 = Parameter.Output<object>("output");
            var results = await processor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);
            Assert.Equal(typeof(string), result[0].ParameterType);
            Assert.True(result["input"].IsBoundToValue);
            Assert.Equal("value", result.Get<string>(callParameter1).BoundValue);
            Assert.Equal(new[] { callParameter1 }, result[callParameter1].BoundParameters);

            Assert.Equal(typeof(object), result[1].ParameterType);
            Assert.True(result["output"].IsBoundToValue);
            Assert.Equal("value", result.Get<object>(callParameter2).BoundValue);
            Assert.Equal(new[] { callParameter2 }, result[callParameter2].BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_WithMapExpressionUsingUnboundParameter_ThrowsException()
        {
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new PredicateParameter[] { Parameter.Predicate<string>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Map<string, object>((PredicateParameter<string>)parameters["parameter1"], x => x))
                    }
                )
            );

            var callParameter1 = Parameter.Output<string>("output1");
            var callParameter2 = Parameter.Output<object>("output2");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync());
            Assert.Equal(new InvalidOperationException("Invalid map expression, parameter 'output1' is not bound to a value.").Message, exception.Message);
        }
    }
}