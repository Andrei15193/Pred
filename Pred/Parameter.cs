using System;

namespace Pred
{
    public class Parameter
    {
        public static Parameter Create<T>()
            => Create<T>(null);

        public static Parameter Create<T>(string name)
            => new Parameter<T>(name);

        public static CallParameter Input<T>(T value)
            => Input<T>(null, value);

        public static CallParameter Input<T>(string name, T value)
            => new InputParameter<T>(name, value);

        public static CallParameter Output<T>()
            => Output<T>(null);

        public static CallParameter Output<T>(string name)
            => new OutputParameter<T>(name);

        internal Parameter(string name, Type parameterType)
        {
            Name = name;
            ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
        }

        public string Name { get; }

        public Type ParameterType { get; }
    }

    public sealed class Parameter<T> : Parameter
    {
        public Parameter(string name)
            : base(name, typeof(T))
        {
        }
    }
}