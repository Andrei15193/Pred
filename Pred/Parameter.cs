using System;
using System.Reflection;

namespace Pred
{
    public class Parameter
    {
        public static PredicateParameter Predicate<T>(string name)
            => new PredicateParameter<T>(name);

        public static InputParameter Input<T>(string name, T value)
            => new InputParameter<T>(name, value);

        internal static InputParameter Input(Type type, object value)
            => (InputParameter)typeof(InputParameter<>)
                .MakeGenericType(type)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, Type.DefaultBinder, new[] { type }, null)
                .Invoke(new object[] { value });

        public static CallParameter Output<T>(string name)
            => new OutputParameter<T>(name);

        internal Parameter(Type parameterType)
            => ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));

        public Type ParameterType { get; }
    }
}