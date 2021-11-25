using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Pred
{
    internal class PredicateProcessorContext
    {
        private readonly Action<ProcessorPredicateProvider> _addPredicateProvider;

        internal PredicateProcessorContext(Predicate predicate, IEnumerable<CallParameter> callParameters, Action<ProcessorPredicateProvider> addPredicateProvider)
        {
            Predicate = predicate;
            CallParameterMapping = callParameters
                .Select((parameter, index) => (Parameter: parameter, Index: index))
                .ToDictionary(pair => predicate.Parameters[pair.Index], pair => pair.Parameter);
            ResultParameterMapping = callParameters
                .ToDictionary(parameter => parameter, _CreateProcessResultParameter);
            _addPredicateProvider = addPredicateProvider;
        }

        internal PredicateProcessorContext(PredicateProcessorContext context)
        {
            Predicate = context.Predicate;
            CallParameterMapping = context.CallParameterMapping;
            ResultParameterMapping = context.ResultParameterMapping.ToDictionary(pair => pair.Key, pair => pair.Value.Clone());
        }

        public Predicate Predicate { get; }

        public IReadOnlyDictionary<PredicateParameter, CallParameter> CallParameterMapping { get; }

        public IDictionary<CallParameter, ResultParameter> ResultParameterMapping { get; }

        internal void AddPredicateProvider(ProcessorPredicateProvider predicateProvider)
            => _addPredicateProvider(predicateProvider);

        private static ResultParameter _CreateProcessResultParameter(CallParameter callParameter)
        {
            var resultParameter = (ResultParameter)Activator.CreateInstance(
                typeof(ResultParameter<>).MakeGenericType(callParameter.ParameterType),
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                new[] { callParameter },
                CultureInfo.InvariantCulture
            );
            if (callParameter is InputParameter inputParameter)
                resultParameter.BindValue(inputParameter.Value);

            return resultParameter;
        }
    }
}