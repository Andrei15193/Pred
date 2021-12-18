using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class ActionPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullCallback_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => PredicateExpression.Action(null));
            Assert.Equal(new ArgumentNullException("callback").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidParameters_InitializesActionExpression()
        {
            var callback = new Action<PredicateExpressionContext>(delegate { });

            var expression = PredicateExpression.Action(callback);
        }
    }
}