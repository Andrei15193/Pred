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
                        new CallPredicateExpression("MyOtherPredicate", new ParameterPredicateExpression(parameters["parameter"]), new ConstantPredicateExpression<int>(20))
                    }),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"]))
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
    }
}