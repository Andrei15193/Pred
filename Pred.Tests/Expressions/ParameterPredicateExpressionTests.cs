using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class ParameterPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("parameter", () => new ParameterPredicateExpression(null));
            Assert.Equal(new ArgumentNullException("parameter").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesValueExpressions()
        {
            var parameter = Parameter.Predicate<int>("parameter");

            var expression = new ParameterPredicateExpression(parameter);

            Assert.Same(parameter, expression.Parameter);
            Assert.Equal(typeof(int), expression.ValueType);
        }
    }
}