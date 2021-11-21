using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pred.Expressions;

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

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameter)
            => ProcessAsync(predicateName, parameter, CancellationToken.None);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, params CallParameter[] parameters)
            => ProcessAsync(predicateName, (IEnumerable<CallParameter>)parameters);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameters, CancellationToken cancellationToken)
        {
            if (predicateName is null)
                throw new ArgumentNullException(nameof(predicateName));
            var parametersList = parameters?.ToArray();
            if (parameters is null || parametersList.Any(parameter => parameter is null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            return _ProcessAsync(predicateName, parametersList, cancellationToken);
        }

        private async IAsyncEnumerable<PredicateProcessResult> _ProcessAsync(string predicateName, IReadOnlyList<CallParameter> parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var predicate in _predicateProvider.GetPredicates(predicateName).WithCancellation(cancellationToken))
                if (_AreParametersMatching(parameters, predicate.Parameters))
                {
                    if (predicate.Body.Count == 0)
                        yield return new PredicateProcessResult(parameters.ToDictionary(
                            parameter => parameter.Name,
                            parameter => parameter is InputParameter inputParameter
                                ? _CreateProcessResultParameter(parameter.Name, parameter.ParameterType, inputParameter.Value)
                                : _CreateProcessResultParameter(parameter.Name, parameter.ParameterType),
                            StringComparer.Ordinal
                        ));
                    else
                    {
                        var bindOrCheckExpression = predicate.Body.Cast<BindOrCheckPredicateExpression>().Single();

                        var parameterIndex = predicate.Parameters.TakeWhile(parameter => parameter != bindOrCheckExpression.Parameter).Count();
                        var callParameter = parameters[parameterIndex];
                        if (!callParameter.IsOutput)
                            throw new NotImplementedException();

                        yield return new PredicateProcessResult(new Dictionary<string, PredicateProcessResultParameter>(StringComparer.Ordinal)
                        {
                            { callParameter.Name, _CreateProcessResultParameter(callParameter.Name, callParameter.ParameterType, bindOrCheckExpression.Value.Value) }
                        });
                    }
                }
        }

        private static PredicateProcessResultParameter _CreateProcessResultParameter(string name, Type parameterType)
            => (PredicateProcessResultParameter)typeof(PredicateProcessResultParameter<>)
                .MakeGenericType(parameterType)
                .GetConstructor(
                    BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
                    Type.DefaultBinder,
                    CallingConventions.Standard,
                    new[] { typeof(string) },
                    Array.Empty<ParameterModifier>()
                )
                .Invoke(new[] { name });

        private static PredicateProcessResultParameter _CreateProcessResultParameter(string name, Type parameterType, object value)
            => (PredicateProcessResultParameter)typeof(PredicateProcessResultParameter<>)
                .MakeGenericType(parameterType)
                .GetConstructor(
                    BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance,
                    Type.DefaultBinder,
                    CallingConventions.Standard,
                    new[] { typeof(string), parameterType },
                    Array.Empty<ParameterModifier>()
                )
                .Invoke(new[] { name, value });

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