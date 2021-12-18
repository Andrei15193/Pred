using System;
using Pred.Expressions;
using Xunit;

namespace Pred.Tests.Expressions
{
    public class MapPredicateExpressionTests
    {
        [Fact]
        public void Create_WithNullSelector_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("selector", () => PredicateExpression.Map<object>(default));
            Assert.Equal(new ArgumentNullException("selector").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesMapExpression()
        {
            var selector = new Func<PredicateExpressionContext, object>(x => x);

            var expression = PredicateExpression.Map<object>(selector);

            Assert.Same(selector, expression.Selector);
        }
    }
}