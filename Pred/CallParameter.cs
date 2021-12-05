using System;

namespace Pred
{
    public abstract class CallParameter : Parameter
    {
        internal CallParameter(string name, Type parameterType)
            : base(parameterType)
            => Name = name ?? throw new ArgumentNullException(nameof(name));

        private protected CallParameter(Type parameterType)
            : base(parameterType)
            => Name = null;

        public string Name { get; }

        public abstract bool IsInput { get; }

        public abstract bool IsOutput { get; }
    }
}