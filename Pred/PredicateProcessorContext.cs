using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Pred.Expressions;

namespace Pred
{
    internal class PredicateProcessorContext
    {
        private readonly Queue<ProcessorPredicateProvider> _additionalPredicateProviders;
        private int _predicateExpressionIndex = -1;

        internal PredicateProcessorContext(Predicate predicate, IEnumerable<CallParameter> callParameters, PredicateProcessorContext baseContext)
        {
            _additionalPredicateProviders = new Queue<ProcessorPredicateProvider>(0);
            Predicate = predicate;
            CallParameterMapping = callParameters
                .Select((parameter, index) => (Parameter: parameter, Index: index))
                .ToDictionary(pair => predicate.Parameters[pair.Index], pair => pair.Parameter);
            ResultParameterMapping = baseContext is null
                ? callParameters.ToDictionary(parameter => parameter, _CreateProcessResultParameter)
                : baseContext.ResultParameterMapping.ToDictionary(pair => pair.Key, pair => pair.Value.Clone());
        }

        public Predicate Predicate { get; }

        public PredicateExpression CurrentExpression
            => Predicate.Body.ElementAtOrDefault(_predicateExpressionIndex);

        public IEnumerable<PredicateExpression> RemainingExpressions
            => Predicate.Body.Skip(_predicateExpressionIndex + 1);

        public bool NextExpression()
        {
            _predicateExpressionIndex++;
            return _predicateExpressionIndex < Predicate.Body.Count;
        }

        public IReadOnlyDictionary<PredicateParameter, CallParameter> CallParameterMapping { get; }

        public IDictionary<CallParameter, ResultParameter> ResultParameterMapping { get; }

        internal IEnumerable<ProcessorPredicateProvider> AdditionalPredicateProviders
            => _additionalPredicateProviders;

        internal void AddPredicateProvider(Func<CancellationToken, IAsyncEnumerable<Predicate>> predicateProvider)
            => _additionalPredicateProviders.Enqueue(new ProcessorPredicateProvider(predicateProvider, this));

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