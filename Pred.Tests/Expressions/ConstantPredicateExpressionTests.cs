using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class ConstantPredicateExpressionTests
    {
        [Fact]
        public void Create_WithValidArguments_InitializesValueExpressions()
        {
            var value = "this is a test";

            var expression = new ConstantPredicateExpression<object>(value);

            Assert.Same(value, expression.Value);
            Assert.Equal(typeof(object), expression.ValueType);
        }
    }
}