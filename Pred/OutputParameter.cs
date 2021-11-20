namespace Pred
{
    public sealed class OutputParameter<T> : CallParameter
    {
        public OutputParameter(string name)
            : base(name, typeof(T))
        {
        }

        public override bool IsInput
            => false;

        public override bool IsOutput
            => true;
    }
}