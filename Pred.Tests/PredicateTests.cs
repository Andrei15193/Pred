using System;
using Xunit;

namespace Pred.Tests
{
    public class PredicateTests
    {
        [Fact]
        public void Create_WithNullName_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("name", () => new Predicate(null));
            Assert.Equal(new ArgumentNullException("name").Message, exception.Message);
        }
    }
}