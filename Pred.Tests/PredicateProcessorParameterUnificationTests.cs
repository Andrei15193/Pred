using System.Threading.Tasks;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorParameterUnificationTests
    {
        [Fact]
        public async Task ProcessAsync_BindingOutputParameterToConstantValue_BindsTheParameter()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Create<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter"], new ValuePredicateExpression<int>(10))
                    }
                )
            );

            var results = await predicateProcessor.ProcessAsync("MyPredicate", Parameter.Output<int>("output")).ToListAsync();

            var result = Assert.Single(results);
            var parameter = Assert.IsType<PredicateProcessResultParameter<int>>(result["output"]);
            Assert.Equal("output", parameter.Name);
            Assert.True(parameter.IsBound);
            Assert.Equal(10, parameter.Value);
        }
    }
}