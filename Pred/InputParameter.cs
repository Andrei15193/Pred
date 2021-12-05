using System;

namespace Pred
{
    public class InputParameter : CallParameter
    {
        internal InputParameter(string name, Type parameterType, object value)
            : base(name, parameterType)
            => Value = value;

        private protected InputParameter(Type parameterType, object value)
            : base(parameterType)
            => Value = value;

        public object Value { get; }

        public sealed override bool IsInput
            => true;

        public sealed override bool IsOutput
            => false;
    }

    public sealed class InputParameter<T> : InputParameter
    {
        public InputParameter(string name, T value)
            : base(name, typeof(T), value)
            => Value = value;

        private InputParameter(T value)
            : base(typeof(T), value)
            => Value = value;

        public new T Value { get; }
    }
}