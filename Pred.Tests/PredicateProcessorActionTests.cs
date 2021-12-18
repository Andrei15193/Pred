using System;
using System.Threading.Tasks;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests
{
    public class PredicateProcessorActionTests
    {
        [Fact]
        public async Task ProcessAsync_WithActionExpression_CanReadInputParameter()
        {
            var callCount = 0;
            var callParameter = Parameter.Input("input", 10);
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.Action(context =>
                        {
                            callCount++;
                            Assert.Same(context.Get<int>("parameter"), context.Get<int>("input"));
                            Assert.True(context.Get<int>(parameters["parameter"]).IsBoundToValue);
                            Assert.Equal(10, context.Get((PredicateParameter<int>)parameters["parameter"]).BoundValue);
                            Assert.Equal(new[] { callParameter }, context.Get((PredicateParameter<int>)parameters["parameter"]).BoundParameters);
                            Assert.Equal(typeof(int), context.Get(callParameter).ParameterType);
                        })
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_WithActionExpression_CanReadOutputParameter()
        {
            var callCount = 0;
            var callParameter = Parameter.Output<int>("output");
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.Action(context =>
                        {
                            callCount++;
                            Assert.Same(context.Get<int>("parameter"), context.Get<int>("output"));
                            Assert.False(context.Get<int>(parameters["parameter"]).IsBoundToValue);
                            Assert.Equal(new[] { callParameter }, context.Get((PredicateParameter<int>)parameters["parameter"]).BoundParameters);
                            Assert.Equal(typeof(int), context.Get(callParameter).ParameterType);
                        })
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_WithActionExpression_CanReadBoundOutputParameter()
        {
            var callCount = 0;
            var callParameter = Parameter.Output<int>("output");
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters => new PredicateExpression[]
                    {
                        PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Constant(20)),
                        PredicateExpression.Action(context =>
                        {
                            callCount++;
                            Assert.Same(context.Get<int>("parameter"), context.Get<int>("output"));
                            Assert.True(context.Get<int>(parameters["parameter"]).IsBoundToValue);
                            Assert.Equal(20, context.Get((PredicateParameter<int>)parameters["parameter"]).BoundValue);
                            Assert.Equal(new[] { callParameter }, context.Get((PredicateParameter<int>)parameters["parameter"]).BoundParameters);
                            Assert.Equal(typeof(int), context.Get(callParameter).ParameterType);
                        })
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_WithActionExpression_CanReadVariable()
        {
            var callCount = 0;
            var callParameter = Parameter.Output<int>("output");
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { Parameter.Predicate<int>("parameter") },
                    parameters =>
                    {
                        var variable = Parameter.Output<int>("variable");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(parameters["parameter"], PredicateExpression.Constant(20)),
                            PredicateExpression.BindOrCheck(variable, PredicateExpression.Constant(30)),
                            PredicateExpression.Action(context =>
                            {
                                callCount++;
                                Assert.Same(context.Get<int>("parameter"), context.Get<int>("output"));
                                Assert.NotSame(context.Get<int>("parameter"), context.Get<int>("variable"));
                                Assert.True(context.Get<int>((OutputParameter)variable).IsBoundToValue);
                                Assert.Equal(30, context.Get(variable).BoundValue);
                                Assert.Equal(new[] { variable }, context.Get(variable).BoundParameters);
                                Assert.Equal(typeof(int), context.Get(variable).ParameterType);
                            })
                        };
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_WithCallExpressionToPredicateWithActionExpression_CanReadRelatedVariable()
        {
            var callCount = 0;
            var parameter = Parameter.Predicate<int>("parameter1");
            var callParameter = Parameter.Output<int>("output");
            var variable = Parameter.Output<int>("variable");
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { parameter },
                    parameters =>
                    {
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(variable, PredicateExpression.Constant(10)),
                            PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(parameters["parameter1"]))
                        };
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { Parameter.Predicate<int>("parameter2") },
                    parameters =>
                    {
                        var innerVariable = Parameter.Output<int>("innerVariable");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Constant(20)),
                            PredicateExpression.BindOrCheck(innerVariable, PredicateExpression.Constant(30)),
                            PredicateExpression.Action(context =>
                            {
                                callCount++;
                                Assert.Same(context.Get<int>("parameter2"), context.Get<int>("output"));
                                Assert.NotSame(context.Get<int>("parameter2"), context.Get<int>("innerVariable"));
                                Assert.True(context.Get<int>((OutputParameter)innerVariable).IsBoundToValue);
                                Assert.Equal(30, context.Get(innerVariable).BoundValue);
                                Assert.Equal(new[] { innerVariable }, context.Get(innerVariable).BoundParameters);
                                Assert.Equal(typeof(int), context.Get(innerVariable).ParameterType);

                                var exception = Assert.Throws<ArgumentException>("name", () => context.Get<int>("variable"));
                                Assert.Equal(new ArgumentException("Parameter/variable 'variable' (predicate parameter or call parameter) could not be found.", "name").Message, exception.Message);

                                exception = Assert.Throws<ArgumentException>("parameter", () => context.Get<int>(parameter));
                                Assert.Equal(new ArgumentException("Predicate parameter 'parameter1' could not be found.", "parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentException>("parameter", () => context.Get<int>(variable));
                                Assert.Equal(new ArgumentException("Call parameter/variable 'variable' could not be found.", "parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("name", () => context.Get<int>(default(string)));
                                Assert.Equal(new ArgumentNullException("name").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("parameter", () => context.Get<int>(default(PredicateParameter)));
                                Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("parameter", () => context.Get<int>(default(CallParameter)));
                                Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
                            })
                        };
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_WithCallExpressionAndActionExpression_CanReadRelatedVariableButNotFromInnerCall()
        {
            var callCount = 0;
            var parameter1 = Parameter.Predicate<int>("parameter1");
            var parameter2 = Parameter.Predicate<int>("parameter2");
            var callParameter = Parameter.Output<int>("output");
            var innerVariable = Parameter.Output<int>("innerVariable");
            var processor = new PredicateProcessor(
                new Predicate(
                    "MyPredicate", new[] { parameter1 },
                    parameters =>
                    {
                        var variable = Parameter.Output<int>("variable");
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(variable, PredicateExpression.Constant(10)),
                            PredicateExpression.Call("MyOtherPredicate", PredicateExpression.Parameter(parameters["parameter1"])),
                            PredicateExpression.Action(context =>
                            {
                                callCount++;
                                Assert.Same(context.Get<int>("parameter1"), context.Get<int>("output"));
                                Assert.NotSame(context.Get<int>("parameter1"), context.Get<int>("variable"));
                                Assert.True(context.Get<int>((OutputParameter)variable).IsBoundToValue);
                                Assert.Equal(10, context.Get(variable).BoundValue);
                                Assert.Equal(new[] { variable }, context.Get(variable).BoundParameters);
                                Assert.Equal(typeof(int), context.Get(variable).ParameterType);

                                var exception = Assert.Throws<ArgumentException>("name", () => context.Get<int>("innerVariable"));
                                Assert.Equal(new ArgumentException("Parameter/variable 'innerVariable' (predicate parameter or call parameter) could not be found.", "name").Message, exception.Message);

                                exception = Assert.Throws<ArgumentException>("parameter", () => context.Get<int>(parameter2));
                                Assert.Equal(new ArgumentException("Predicate parameter 'parameter2' could not be found.", "parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentException>("parameter", () => context.Get<int>(innerVariable));
                                Assert.Equal(new ArgumentException("Call parameter/variable 'innerVariable' could not be found.", "parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("name", () => context.Get<int>(default(string)));
                                Assert.Equal(new ArgumentNullException("name").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("parameter", () => context.Get<int>(default(PredicateParameter)));
                                Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);

                                exception = Assert.Throws<ArgumentNullException>("parameter", () => context.Get<int>(default(CallParameter)));
                                Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
                            })
                        };
                    }
                ),
                new Predicate(
                    "MyOtherPredicate", new[] { parameter2 },
                    parameters =>
                    {
                        return new PredicateExpression[]
                        {
                            PredicateExpression.BindOrCheck(parameters["parameter2"], PredicateExpression.Constant(20)),
                            PredicateExpression.BindOrCheck(innerVariable, PredicateExpression.Constant(30))
                        };
                    }
                )
            );

            var results = await processor.ProcessAsync("MyPredicate", callParameter).ToListAsync();

            Assert.Single(results);
            Assert.Equal(1, callCount);
        }
    }
}