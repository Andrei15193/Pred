using System;
using Xunit;

namespace Pred.Tests
{
    public class InputParameterTests
    {
        [Fact]
        public void Create_WithValidArguments_InitializesParameter()
        {
            var parameter = new InputParameter<int>("parameter", 10);

            Assert.Equal("parameter", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);
            Assert.Equal(10, parameter.Value);
            Assert.True(parameter.IsInput);
            Assert.False(parameter.IsOutput);
        }
    }
}