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
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new BindOrCheckPredicateExpression(null, new ConstantPredicateExpression<int>(10)));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullValueExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("valueExpression", () => new BindOrCheckPredicateExpression(Parameter.Predicate<int>("parameter"), default(ValuePredicateExpression)));
            Assert.Equal(new ArgumentNullException("valueExpression").Message, exception.Message);
        }

        [Fact]
        public void Create_WithBaseValueTypeBindingToConcreteType_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("valueExpression", () => new BindOrCheckPredicateExpression(Parameter.Predicate<int>("parameter"), new ConstantPredicateExpression<object>(default)));
            Assert.Equal(new ArgumentException("Cannot assign value of type 'System.Object' (value) to 'System.Int32' (parameter).", "valueExpression").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesBindOrCheckExpression()
        {
            var parameter = Parameter.Predicate<object>("parameter");
            var value = new ConstantPredicateExpression<int>(default);

            var expression = new BindOrCheckPredicateExpression(parameter, value);

            Assert.Same(parameter, expression.Parameter);
            Assert.Same(value, expression.Value);
        }
    }
}