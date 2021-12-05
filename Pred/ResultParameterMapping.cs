using System;

namespace Pred
{
    internal sealed class ResultParameterMapping
    {
        public ResultParameterMapping(CallParameter callParameter, ResultParameter resultParameter)
        {
            CallParameter = callParameter ?? throw new ArgumentNullException(nameof(callParameter));
            ResultParameter = resultParameter ?? throw new ArgumentNullException(nameof(resultParameter));
        }

        public CallParameter CallParameter { get; }

        public ResultParameter ResultParameter { get; }
    }
}