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
            var exception = Assert.Throws<ArgumentNullException>("name", () => PredicateExpression.Call(null));
            Assert.Equal(new ArgumentNullException("name").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameters_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => PredicateExpression.Call("", default(ValuePredicateExpression[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => PredicateExpression.Call("", default(ValuePredicateExpression)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesTheCallExpression()
        {
            var parameter1 = PredicateExpression.Constant<int>(10);
            var parameter2 = PredicateExpression.Constant<object>("test");

            var expression = PredicateExpression.Call("MyPredicate", parameter1, parameter2);

            Assert.Equal("MyPredicate", expression.Name);
            Assert.Equal(new ValuePredicateExpression[] { parameter1, parameter2 }, expression.Parameters);
        }
    }
}