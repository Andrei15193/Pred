using System;
using System.Collections.Generic;
using System.Linq;
using Pred.Expressions;
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
            var exception = Assert.Throws<ArgumentException>("parameters", () => new Predicate("", default(PredicateParameter)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullParameters_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("parameters", () => new Predicate("", default(PredicateParameter[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null parameters.", "parameters").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullBody_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("body", () => new Predicate("", Enumerable.Empty<PredicateParameter>(), default(PredicateExpression)));
            Assert.Equal(new ArgumentException("Cannot be null or contain null expressions.", "body").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullBodyExpression_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("body", () => new Predicate("", Enumerable.Empty<PredicateParameter>(), default(PredicateExpression[])));
            Assert.Equal(new ArgumentException("Cannot be null or contain null expressions.", "body").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullBodyProvider_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("bodyProvider", () => new Predicate("", Enumerable.Empty<PredicateParameter>(), default(Func<IReadOnlyDictionary<string, PredicateParameter>, IEnumerable<PredicateExpression>>)));
            Assert.Equal(new ArgumentException("Cannot be null, return null or return expressions contain null.", "bodyProvider").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullBodyFromBodyProvider_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("bodyProvider", () => new Predicate("", Enumerable.Empty<PredicateParameter>(), parameters => new[] { default(PredicateExpression) }));
            Assert.Equal(new ArgumentException("Cannot be null, return null or return expressions contain null.", "bodyProvider").Message, exception.Message);
        }

        [Fact]
        public void Create_WithNullBodyExpressionFromBodyProvider_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>("bodyProvider", () => new Predicate("", Enumerable.Empty<PredicateParameter>(), parameters => default(PredicateExpression[])));
            Assert.Equal(new ArgumentException("Cannot be null, return null or return expressions contain null.", "bodyProvider").Message, exception.Message);
        }

        [Fact]
        public void Create_WithValidArguments_InitializesPredicate()
        {
            var parameter1 = new PredicateParameter<int>("parameter1");
            var parameter2 = new PredicateParameter<object>("parameter2");
            var expression1 = PredicateExpression.Constant<int>(10);
            var expression2 = PredicateExpression.Constant<object>(20);

            var predicate = new Predicate("predicate", new PredicateParameter[] { parameter1, parameter2 }, expression1, expression2);

            Assert.Equal("predicate", predicate.Name);
            Assert.Equal(new PredicateParameter[] { parameter1, parameter2 }, predicate.Parameters);
            Assert.Equal(new PredicateExpression[] { expression1, expression2 }, predicate.Body);
        }
    }
}