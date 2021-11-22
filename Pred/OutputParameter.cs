using System;

namespace Pred
{
    public class OutputParameter : CallParameter
    {
        internal OutputParameter(string name, Type parameterType)
            : base(name, parameterType)
        {
        }

        public sealed override bool IsInput
            => false;

        public sealed override bool IsOutput
            => true;
    }

    public sealed class OutputParameter<T> : OutputParameter
    {
        public OutputParameter(string name)
            : base(name, typeof(T))
        {
        }
    }
}