using System.Collections.Generic;

namespace Pred
{
    public class PredicateProcessResult
    {
        private readonly IReadOnlyDictionary<string, PredicateProcessResultParameter> _result;

        internal PredicateProcessResult(IReadOnlyDictionary<string, PredicateProcessResultParameter> result)
            => _result = result;

        public PredicateProcessResultParameter this[string name]
            => _result[name];
    }
}