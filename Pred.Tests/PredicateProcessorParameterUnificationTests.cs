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
                    "MyPredicate", new[] { Parameter.Predicate<object>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter"], new ConstantPredicateExpression<int>(10))
                    }
                )
            );

            var callParameter = Parameter.Output<object>("output");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            var result = Assert.Single(results);
            var resultParameter = Assert.IsType<ResultParameter<object>>(result["output"]);
            Assert.True(resultParameter.IsBoundToValue);
            Assert.Equal(new[] { callParameter }, resultParameter.BoundParameters);
            Assert.Equal(10, resultParameter.BoundBalue);
            Assert.Equal(typeof(object), resultParameter.ParameterType);
        }

        [Fact]
        public async Task ProcessAsync_BindingOutputParameters_BindsThemToTheSameParameter()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"]))
                    }
                )
            );

            var callParameter1 = Parameter.Output<object>("output1");
            var callParameter2 = Parameter.Output<object>("output2");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result["output1"], result["output2"]);
            Assert.False(result[0].IsBoundToValue);
            Assert.Equal(new[] { callParameter1, callParameter2 }, result[callParameter1].BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_BindingOutputParametersAndThenBindingOneOfThemToAValue_BindsThemBothToTheSameValue()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"])),
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ConstantPredicateExpression<int>(10))
                    }
                )
            );

            var callParameter1 = Parameter.Output<object>("output1");
            var callParameter2 = Parameter.Output<object>("output2");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result["output1"], result["output2"]);
            Assert.True(result["output1"].IsBoundToValue);
            Assert.Equal(10, result["output1"].BoundBalue);
            Assert.Equal(new[] { callParameter1, callParameter2 }, result[callParameter1].BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_BindingOneOutputParameterToAValueAndThenBetweenThem_BindsThemBothToTheSameValue()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<object>("parameter1"), Parameter.Predicate<object>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ConstantPredicateExpression<int>(10)),
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"]))
                    }
                )
            );

            var callParameter1 = Parameter.Output<object>("output1");
            var callParameter2 = Parameter.Output<object>("output2");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result["output1"], result["output2"]);
            Assert.True(result["output1"].IsBoundToValue);
            Assert.Equal(10, result["output1"].BoundBalue);
            Assert.Equal(new[] { callParameter1, callParameter2 }, result[callParameter1].BoundParameters);
        }

        [Fact]
        public async Task ProcessAsync_BindingSameParameterToItself_HasNoEffect()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter1"]))
                    }
                )
            );

            var callParameter1 = Parameter.Output<object>("output1");
            var callParameter2 = Parameter.Output<object>("output2");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter1, callParameter2).ToListAsync();

            var result = Assert.Single(results);

            var resultParameter1 = Assert.IsType<ResultParameter<object>>(result["output1"]);
            Assert.False(resultParameter1.IsBoundToValue);
            Assert.Equal(new[] { callParameter1 }, resultParameter1.BoundParameters);
            Assert.Equal(typeof(object), resultParameter1.ParameterType);

            var resultParameter2 = Assert.IsType<ResultParameter<object>>(result["output2"]);
            Assert.False(resultParameter2.IsBoundToValue);
            Assert.Equal(new[] { callParameter2 }, resultParameter2.BoundParameters);
            Assert.Equal(typeof(object), resultParameter2.ParameterType);
        }

        [Fact]
        public async Task ProcessAsync_BindingMultipleParametersWithEachOther_BindsThemAllToTheSameValue()
        {
            var predicateProcessor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter1"), Parameter.Predicate<int>("parameter2"), Parameter.Predicate<int>("parameter3") },
                    parameters => new PredicateExpression[]
                    {
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"])),
                        new BindOrCheckPredicateExpression(parameters["parameter1"], new ParameterPredicateExpression(parameters["parameter2"])),
                        new BindOrCheckPredicateExpression(parameters["parameter2"], new ParameterPredicateExpression(parameters["parameter3"])),
                        new BindOrCheckPredicateExpression(parameters["parameter3"], new ConstantPredicateExpression<int>(10))
                    }
                )
            );

            var callParameter1 = Parameter.Output<object>("output1");
            var callParameter2 = Parameter.Output<object>("output2");
            var callParameter3 = Parameter.Output<object>("output3");
            var results = await predicateProcessor.ProcessAsync("MyPredicate", callParameter1, callParameter2, callParameter3).ToListAsync();

            var result = Assert.Single(results);
            Assert.Same(result["output1"], result["output2"]);
            Assert.Same(result["output2"], result["output3"]);
            Assert.True(result["output1"].IsBoundToValue);
            Assert.Equal(10, result["output1"].BoundBalue);
            Assert.Equal(new[] { callParameter1, callParameter2, callParameter3 }, result[callParameter1].BoundParameters);
        }
    }
}