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

        [Fact]
        public void Create_WithNullParameter_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => new Predicate("", default(Parameter)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameters_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => new Predicate("", default(Parameter[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesPredicate()
        {
            var predicate = new Predicate("predicate", new Parameter<int>("parameter1"), new Parameter<object>("parameter2"));

            Assert.Equal("predicate", predicate.Name);
            Assert.Equal(2, predicate.Parameters.Count);
            Assert.Equal("parameter1", predicate.Parameters[0].Name);
            Assert.Equal(typeof(int), predicate.Parameters[0].ParameterType);
            Assert.Equal("parameter2", predicate.Parameters[1].Name);
            Assert.Equal(typeof(object), predicate.Parameters[1].ParameterType);
        }
    }
}