using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public class ResultParameter : Parameter
    {
        private object _boundBalue;
        private readonly List<CallParameter> _boundParameters;

        internal ResultParameter(CallParameter callParameter)
            : base(callParameter?.ParameterType ?? throw new ArgumentNullException(nameof(callParameter)))
            => _boundParameters = new List<CallParameter>(1) { callParameter };

        public bool IsBoundToValue { get; private set; }

        public object BoundBalue
            => IsBoundToValue ? _boundBalue : throw new InvalidOperationException("The parameter is not bound to a value.");

        public IReadOnlyList<CallParameter> BoundParameters
            => _boundParameters;

        internal void BindParameter(ResultParameter resultParameter)
        {
            if (this != resultParameter)
                _boundParameters.AddRange(resultParameter.BoundParameters.Where(boundParameter => !_boundParameters.Contains(boundParameter)));
        }

        internal virtual void BindValue(object value)
        {
            if (IsBoundToValue)
                throw new InvalidOperationException("The parameter is already bound to a value.");
            if (value is null && ParameterType.IsValueType && Nullable.GetUnderlyingType(ParameterType) is null)
                throw new InvalidOperationException("Expected a value type, instead received null.");
            if (!ParameterType.IsAssignableFrom(value?.GetType() ?? typeof(object)))
                throw new InvalidOperationException($"Cannot assing value of type '{value?.GetType() ?? typeof(object)}' to parameter of type '{ParameterType}'.");

            IsBoundToValue = true;
            _boundBalue = value;
        }
    }

    public sealed class ResultParameter<T> : ResultParameter
    {
        private T _boundBalue;

        internal ResultParameter(CallParameter callParameter)
            : base(callParameter)
        {
        }

        public new T BoundBalue
            => IsBoundToValue ? _boundBalue : throw new InvalidOperationException("The parameter is not bound to a value.");

        internal override void BindValue(object value)
        {
            _boundBalue = (T)value;
            base.BindValue(_boundBalue);
        }
    }
}