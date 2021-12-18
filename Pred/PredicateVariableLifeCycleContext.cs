using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Pred
{
    internal sealed class PredicateVariableLifeCycleContext
    {
        private readonly List<CallParameter> _localVariables;

        public PredicateVariableLifeCycleContext(IEnumerable<PredicateParameterMapping> parameterMappings)
        {
            if (parameterMappings is null || parameterMappings.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameter mappings.", nameof(parameterMappings));

            _localVariables = new List<CallParameter>();
            CallParameterMapping = parameterMappings.ToDictionary(mapping => mapping.Parameter, mapping => mapping.CallParameter);
            ResultParameterMapping = parameterMappings.Select(mapping => mapping.CallParameter).Distinct().ToDictionary(callParameter => callParameter, _CreateProcessResultParameter);
        }

        public IReadOnlyDictionary<PredicateParameter, CallParameter> CallParameterMapping { get; }

        public IDictionary<CallParameter, ResultParameter> ResultParameterMapping { get; }

        public CallParameter GetOrAddCallParameter(Parameter parameter)
            => GetOrAddCallParameter(parameter, out var _);

        public CallParameter GetOrAddCallParameter(Parameter parameter, out ResultParameter resultParameter)
        {
            if (parameter is PredicateParameter expressionPredicateParameter)
            {
                var callParameter = CallParameterMapping[expressionPredicateParameter];
                resultParameter = ResultParameterMapping[callParameter];
                return callParameter;
            }
            else if (parameter is CallParameter expressionCallParameter)
            {
                resultParameter = AddVariable(expressionCallParameter);
                return expressionCallParameter;
            }
            else
                throw new InvalidOperationException($"Unhandled parameter type '{parameter.GetType()}'.");
        }

        public ResultParameter AddVariable(CallParameter callParameter)
        {
            if (!ResultParameterMapping.TryGetValue(callParameter, out var resultParameter))
            {
                 resultParameter = _CreateProcessResultParameter(callParameter);
                _localVariables.Add(callParameter);
                ResultParameterMapping.Add(callParameter, resultParameter);
            }
            return resultParameter;
        }

        public void ClearVariables()
        {
            foreach (var localVariable in _localVariables)
            {
                ResultParameterMapping[localVariable].UnbindParameter(localVariable);
                ResultParameterMapping.Remove(localVariable);
            }
            _localVariables.Clear();
        }

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