using System;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public class ResultParameter : Parameter
    {
        private object _boundValue;
        private readonly List<CallParameter> _boundParameters;

        internal ResultParameter(CallParameter callParameter)
            : base(callParameter?.ParameterType ?? throw new ArgumentNullException(nameof(callParameter)))
            => _boundParameters = new List<CallParameter>(1) { callParameter };

        private protected ResultParameter(ResultParameter resultParameter)
            : base(resultParameter?.ParameterType ?? throw new ArgumentNullException(nameof(resultParameter)))
        {
            _boundValue = resultParameter._boundValue;
            IsBoundToValue = resultParameter.IsBoundToValue;
            _boundParameters = new List<CallParameter>(resultParameter.BoundParameters);
        }

        public bool IsBoundToValue { get; private set; }

        public object BoundValue
            => IsBoundToValue ? _boundValue : throw new InvalidOperationException("The parameter is not bound to a value.");

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
            _boundValue = value;
        }

        internal virtual ResultParameter Clone()
            => new ResultParameter(this);
    }

    public sealed class ResultParameter<T> : ResultParameter
    {
        private T _boundValue;

        internal ResultParameter(CallParameter callParameter)
            : base(callParameter)
        {
        }

        private ResultParameter(ResultParameter<T> resultParameter)
            : base(resultParameter)
            => _boundValue = resultParameter._boundValue;

        public new T BoundValue
            => IsBoundToValue ? _boundValue : throw new InvalidOperationException("The parameter is not bound to a value.");

        internal override void BindValue(object value)
        {
            _boundValue = (T)value;
            base.BindValue(_boundValue);
        }

        internal override ResultParameter Clone()
            => new ResultParameter<T>(this);
    }
}