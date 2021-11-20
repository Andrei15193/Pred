using System;
using Xunit;

namespace Pred.Tests
{
    public class OutputParameterTests
    {
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