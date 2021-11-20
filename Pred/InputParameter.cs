namespace Pred
{
    public sealed class InputParameter<T> : CallParameter
    {
        public InputParameter(string name, T value)
            : base(name, typeof(T))
            => Value = value;

        public T Value { get; }

        public override bool IsInput
            => true;

        public override bool IsOutput
            => false;
    }
}