using System;
using Xunit;

namespace Pred.Tests
{
    public class ParameterTests
    {
        [Fact]
        public void Create_WithNullName_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("name", () => new PredicateParameter<int>(null));
            Assert.Equal(new ArgumentNullException("name").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesParameter()
        {
            var parameter = new PredicateParameter<int>("parameter");

            Assert.Equal("parameter", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);
        }
    }
}