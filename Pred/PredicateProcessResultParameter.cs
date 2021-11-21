using System;

namespace Pred
{
    public class PredicateProcessResultParameter : Parameter
    {
        private readonly object _value;

        internal PredicateProcessResultParameter(string name, Type parameterType)
            : base(name, parameterType)
            => IsBound = false;

        internal PredicateProcessResultParameter(string name, Type parameterType, object value)
            : base(name, parameterType)
            => (_value, IsBound) = (value, true);

        public object Value
            => IsBound ? _value : throw new InvalidOperationException("The parameter is not bound.");

        public bool IsBound { get; }
    }

    public sealed class PredicateProcessResultParameter<T> : PredicateProcessResultParameter
    {
        private readonly T _value;

        internal PredicateProcessResultParameter(string name)
            : base(name, typeof(T))
        {
        }

        internal PredicateProcessResultParameter(string name, T value)
            : base(name, typeof(T), value)
            => _value = value;

        public new T Value
            => IsBound ? _value : throw new InvalidOperationException("The parameter is not bound.");
    }
}