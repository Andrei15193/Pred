using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public abstract class CallParameter : Parameter
    {
        internal static bool AreParametersMatching(IReadOnlyList<CallParameter> callParameters, IReadOnlyList<Parameter> predicateParameters)
            => callParameters.Count == predicateParameters.Count
                && callParameters
                .Zip(predicateParameters, (callParameter, predicateParameter) => (CallParameter: callParameter, PredicateParameter: predicateParameter))
                .All(
                    mapping => mapping.CallParameter.IsInput
                        ? mapping.PredicateParameter.ParameterType.IsAssignableFrom(mapping.CallParameter.ParameterType)
                        : mapping.CallParameter.IsOutput
                        ? mapping.CallParameter.ParameterType.IsAssignableFrom(mapping.PredicateParameter.ParameterType)
                        : false
                );

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