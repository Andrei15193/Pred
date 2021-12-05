using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pred
{
    public sealed class PredicateProcessResult : IEnumerable<ResultParameter>
    {
        private readonly IReadOnlyList<ResultParameterMapping> _result;

        internal PredicateProcessResult(IEnumerable<ResultParameterMapping> parameters)
        {
            _result = parameters as IReadOnlyList<ResultParameterMapping> ?? parameters?.ToArray();

            if (_result is null || _result.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameter mappings.", nameof(parameters));
        }

        public ResultParameter this[int index]
            => _result[index].ResultParameter;

        public ResultParameter this[string name]
        {
            get
            {
                if (name is null)
                    throw new ArgumentNullException(nameof(name));

                return _result.Single(result => string.Equals(result.CallParameter.Name, name, StringComparison.Ordinal)).ResultParameter;
            }
        }

        public ResultParameter this[CallParameter callParameter]
        {
            get
            {
                if (callParameter is null)
                    throw new ArgumentNullException(nameof(callParameter));

                return _result.Single(result => result.CallParameter == callParameter).ResultParameter;
            }
        }

        public ResultParameter<T> GetAt<T>(int index)
            => (ResultParameter<T>)this[index];

        public ResultParameter<T> Get<T>(string name)
            => (ResultParameter<T>)this[name];

        public ResultParameter<T> Get<T>(CallParameter callParameter)
            => (ResultParameter<T>)this[callParameter];

        public IEnumerator<ResultParameter> GetEnumerator()
            => _result.Select(result => result.ResultParameter).Distinct().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}