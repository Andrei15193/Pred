using System;
using Xunit;

namespace Pred.Tests
{
    public class OutputParameterTests
    {
        [Fact]
        public void Create_WithNullName_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>("name", () => new OutputParameter<int>(null));
            Assert.Equal(new ArgumentNullException("name").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesParameter()
        {
            var parameter = new OutputParameter<int>("parameter");

            Assert.Equal("parameter", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);
            Assert.False(parameter.IsInput);
            Assert.True(parameter.IsOutput);
        }
    }
}