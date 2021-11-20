using System;

namespace Pred
{
    public abstract class CallParameter : Parameter
    {
        internal CallParameter(string name, Type parameterType)
            : base(name, parameterType)
        {
        }

        public abstract bool IsInput { get; }

        public abstract bool IsOutput { get; }
    }
}