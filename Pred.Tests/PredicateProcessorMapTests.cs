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
                        PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Map<object>(context => context.Get<string>("parameter1").BoundValue))
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
    }
}