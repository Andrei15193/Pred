using System;
using System.Linq;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class CallPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullName_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("name", () => new CallPredicateExpression(null));
            Assert.Equal(new ArgumentNullException("name").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameters_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => new CallPredicateExpression("", default(ValuePredicateExpression[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => new CallPredicateExpression("", default(ValuePredicateExpression)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesTheCallExpression()
        {
            var parameter1 = new ConstantPredicateExpression<int>(10);
            var parameter2 = new ConstantPredicateExpression<object>("test");

            var expression = new CallPredicateExpression("MyPredicate", parameter1, parameter2);

            Assert.Equal("MyPredicate", expression.Name);
            Assert.Equal(new ValuePredicateExpression[] { parameter1, parameter2 }, expression.Parameters);
        }
    }
}