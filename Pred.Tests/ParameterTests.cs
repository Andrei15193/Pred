using System;
using Xunit;

namespace Pred.Tests
{
    public class ParameterTests
    {
        [Fact]
        public void Create_WithValidArguments_InitializesParameter()
        {
            var parameter = new Parameter<int>("parameter");

            Assert.Equal("parameter", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);
        }
    }
}