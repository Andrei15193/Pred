using System;

namespace Pred
{
    public class PredicateParameter : Parameter
    {
        internal PredicateParameter(string name, Type parameterType)
            : base(parameterType)
            => Name = name ?? throw new ArgumentNullException(nameof(name));

        public string Name { get; }
    }

    public sealed class PredicateParameter<T> : PredicateParameter
    {
        public PredicateParameter(string name)
            : base(name, typeof(T))
        {
        }
    }
}