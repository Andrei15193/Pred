using System;

namespace Pred
{
    public class Parameter
    {
        public static PredicateParameter Predicate<T>(string name)
            => new PredicateParameter<T>(name);

        public static CallParameter Input<T>(string name, T value)
            => new InputParameter<T>(name, value);

        public static CallParameter Output<T>(string name)
            => new OutputParameter<T>(name);

        internal Parameter(Type parameterType)
        {
            ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
        }

        public Type ParameterType { get; }
    }
}