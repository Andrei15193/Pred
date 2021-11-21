using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class BindOrCheckPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new BindOrCheckPredicateExpression(null, new ValuePredicateExpression<int>(10)));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullValue_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("value", () => new BindOrCheckPredicateExpression(Parameter.Create<int>("parameter"), null));
            Assert.Equal(new ArgumentNullException("value").Message, exception.Message);
        }

        [Fact]
        public void Create_WithBaseValueTypeBindingToConcreteType_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("value", () => new BindOrCheckPredicateExpression(Parameter.Create<int>("parameter"), new ValuePredicateExpression<object>(default)));
            Assert.Equal(new ArgumentException("Cannot assign value of type 'System.Object' (value) to 'System.Int32' (parameter).", "value").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesBindOrCheckExpression()
        {
            var parameter = Parameter.Create<object>("parameter");
            var value = new ValuePredicateExpression<int>(default);

            var expression = new BindOrCheckPredicateExpression(parameter, value);

            Assert.Same(parameter, expression.Parameter);
            Assert.Same(value, expression.Value);
        }
    }
}