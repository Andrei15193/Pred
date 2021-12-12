using System;
using System.Linq;

namespace Pred.Expressions
{
    public class PredicateExpressionContext
    {
        private readonly PredicateVariableLifeCycleContext _lifeCycleContext;

        internal PredicateExpressionContext(PredicateVariableLifeCycleContext lifeCycleContext)
            => _lifeCycleContext = lifeCycleContext ?? throw new ArgumentNullException(nameof(lifeCycleContext));

        public ResultParameter<T> Get<T>(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var matchingResultParameters = _lifeCycleContext
                .CallParameterMapping
                .Where(pair => string.Equals(pair.Key.Name, name, StringComparison.Ordinal))
                .Select(pair => _lifeCycleContext.ResultParameterMapping[pair.Value])
                .Concat(
                    _lifeCycleContext
                        .ResultParameterMapping
                        .Where(pair => string.Equals(pair.Key.Name, name, StringComparison.Ordinal))
                        .Select(pair => pair.Value)
                );
            if (!matchingResultParameters.Any() || matchingResultParameters.Skip(1).Any())
                throw new ArgumentException($"Parameter/variable '{name}' (predicate parameter or call parameter) could not be found.", nameof(name));

            return (ResultParameter<T>)matchingResultParameters.Single();
        }

        public ResultParameter<T> Get<T>(PredicateParameter parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            if (!_lifeCycleContext.CallParameterMapping.TryGetValue(parameter, out var callParameter))
                throw new ArgumentException($"Predicate parameter '{parameter.Name}' could not be found.", nameof(parameter));
            else
                return (ResultParameter<T>)_lifeCycleContext.ResultParameterMapping[callParameter];
        }

        public ResultParameter<T> Get<T>(PredicateParameter<T> parameter)
            => Get<T>((PredicateParameter)parameter);

        public ResultParameter<T> Get<T>(CallParameter parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            if (!_lifeCycleContext.ResultParameterMapping.TryGetValue(parameter, out var resultParameter))
                throw new ArgumentException($"Call parameter/variable '{parameter.Name}' could not be found.", nameof(parameter));
            else
                return (ResultParameter<T>)resultParameter;
        }

        public ResultParameter<T> Get<T>(InputParameter<T> parameter)
            => Get<T>((CallParameter)parameter);

        public ResultParameter<T> Get<T>(OutputParameter<T> parameter)
            => Get<T>((CallParameter)parameter);
    }
}