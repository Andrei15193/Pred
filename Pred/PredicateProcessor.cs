using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pred
{
    public class PredicateProcessor
    {
        private readonly IPredicateProvider _predicateProvider;

        public PredicateProcessor(IPredicateProvider predicateProvider)
            => _predicateProvider = predicateProvider;

        public PredicateProcessor(IEnumerable<Predicate> predicates)
            : this(new InMemoryPredicateProvider(predicates))
        {
        }

        public PredicateProcessor(params Predicate[] predicates)
            : this((IEnumerable<Predicate>)predicates)
        {
        }

        public IAsyncEnumerable<object> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameter)
            => ProcessAsync(predicateName, parameter, CancellationToken.None);

        public IAsyncEnumerable<object> ProcessAsync(string predicateName, params CallParameter[] parameters)
            => ProcessAsync(predicateName, (IEnumerable<CallParameter>)parameters);

        public IAsyncEnumerable<object> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameters, CancellationToken cancellationToken)
        {
            if (predicateName is null)
                throw new ArgumentNullException(nameof(predicateName));
            var parametersList = parameters?.ToArray();
            if (parameters is null || parametersList.Any(parameter => parameter is null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            return _ProcessAsync(predicateName, parametersList, cancellationToken);
        }

        private async IAsyncEnumerable<object> _ProcessAsync(string predicateName, IReadOnlyList<CallParameter> parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var predicate in _predicateProvider.GetPredicates(predicateName).WithCancellation(cancellationToken))
                if (_AreParametersMatching(parameters, predicate.Parameters))
                    yield return predicate;
        }

        private static bool _AreParametersMatching(IReadOnlyList<CallParameter> callParameters, IReadOnlyList<Parameter> predicateParameters)
            => callParameters.Count == predicateParameters.Count
                && callParameters
                .Zip(predicateParameters, (callParameter, predicateParameter) => (CallParameter: callParameter, PredicateParameter: predicateParameter))
                .All(
                    pair => pair.CallParameter.IsInput
                        ? pair.PredicateParameter.ParameterType.IsAssignableFrom(pair.CallParameter.ParameterType)
                        : pair.CallParameter.IsOutput
                        ? pair.CallParameter.ParameterType.IsAssignableFrom(pair.PredicateParameter.ParameterType)
                        : false
                );
    }
}