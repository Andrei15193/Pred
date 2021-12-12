using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class CheckPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullCallback_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => PredicateExpression.Check(null));
            Assert.Equal(new ArgumentNullException("callback").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesCheckExpression()
        {
            var callback = new Func<PredicateExpressionContext, bool>(delegate { return true; });

            var expression = PredicateExpression.Check(callback);
        }
    }
}