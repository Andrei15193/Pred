using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class MapPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullParameterExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameterExpression", () => new MapPredicateExpression<string, object>(default(ParameterPredicateExpression), x => x));
            Assert.Equal(new ArgumentNullException("parameterExpression").Message, exception.Message);
        }

        [Fact]
        public void Create_WithParameterExpressionAndNullSelectorExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("selector", () => new MapPredicateExpression<string, object>(new ParameterPredicateExpression(Parameter.Predicate<string>("parameter")), default));
            Assert.Equal(new ArgumentNullException("selector").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameterExpressionAndSelector_InitializesMapExpression()
        {
            var parameterExpression = new ParameterPredicateExpression(Parameter.Predicate<string>("parameter"));
            var selector = new Func<string, object>(x => x);

            var expression = new MapPredicateExpression<string, object>(parameterExpression, selector);

            Assert.Same(parameterExpression, expression.ParameterExpression);
            Assert.Same(selector, expression.Selector);
        }

        [Fact]
        public void Create_WithNullPredicateParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new MapPredicateExpression<string, object>(default(PredicateParameter<string>), x => x));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullPredicateParameterAndSelectorExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("selector", () => new MapPredicateExpression<string, object>(Parameter.Predicate<string>("parameter"), default));
            Assert.Equal(new ArgumentNullException("selector").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidPredicateParameterAndSelector_InitializesMapExpression()
        {
            var parameter = Parameter.Predicate<string>("parameter");
            var selector = new Func<string, object>(x => x);

            var expression = new MapPredicateExpression<string, object>(parameter, selector);

            Assert.Same(parameter, expression.ParameterExpression.Parameter);
            Assert.Same(selector, expression.Selector);
        }

        [Fact]
        public void Create_WithNullInputParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new MapPredicateExpression<string, object>(default(InputParameter<string>), x => x));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullInputParameterAndSelectorExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("selector", () => new MapPredicateExpression<string, object>(Parameter.Input<string>("parameter", default), default));
            Assert.Equal(new ArgumentNullException("selector").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidInputParameterAndSelector_InitializesMapExpression()
        {
            var parameter = Parameter.Input<string>("parameter", default);
            var selector = new Func<string, object>(x => x);

            var expression = new MapPredicateExpression<string, object>(parameter, selector);

            Assert.Same(parameter, expression.ParameterExpression.Parameter);
            Assert.Same(selector, expression.Selector);
        }

        [Fact]
        public void Create_WithNullOutputParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new MapPredicateExpression<string, object>(default(OutputParameter<string>), x => x));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullOutputParameterAndSelectorExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("selector", () => new MapPredicateExpression<string, object>(Parameter.Output<string>("parameter"), default));
            Assert.Equal(new ArgumentNullException("selector").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidOutputParameterAndSelector_InitializesMapExpression()
        {
            var parameter = Parameter.Output<string>("parameter");
            var selector = new Func<string, object>(x => x);

            var expression = new MapPredicateExpression<string, object>(parameter, selector);

            Assert.Same(parameter, expression.ParameterExpression.Parameter);
            Assert.Same(selector, expression.Selector);
        }
    }
}