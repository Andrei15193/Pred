using System;

namespace Pred
{
    internal sealed class PredicateParameterMapping
    {
        public PredicateParameterMapping(PredicateParameter parameter, CallParameter callParameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            CallParameter = callParameter ?? throw new ArgumentNullException(nameof(callParameter));
        }

        public PredicateParameter Parameter { get; }

        public CallParameter CallParameter { get; }
    }
}