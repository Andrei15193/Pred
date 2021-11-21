using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class ValuePredicateExpressionTests
    {
        [Fact]
        public void Create_WithValidArguments_InitializesValueExpressions()
        {
            var value = "this is a test";

            var expression = new ValuePredicateExpression<object>(value);

            Assert.Same(value, expression.Value);
            Assert.Equal(typeof(object), expression.ValueType);
        }
    }
}