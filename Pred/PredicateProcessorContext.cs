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
        private int _predicateExpressionIndex;
        private readonly Action<ProcessorPredicateProvider> _addAdditionalPredicateProviderCallback;
        private readonly Stack<PredicateVariableLifeCycleContext> _variableLifeCycleContexts;

        internal PredicateProcessorContext(Predicate predicate, IPredicateProvider predicateProvider, Action<ProcessorPredicateProvider> addAdditionalPredicateProviderCallback)
        {
            _addAdditionalPredicateProviderCallback = addAdditionalPredicateProviderCallback;
            _variableLifeCycleContexts = new Stack<PredicateVariableLifeCycleContext>();
            _predicateExpressionIndex = -1;
            Predicate = predicate;
            PredicateProvider = predicateProvider;
        }

        public Predicate Predicate { get; }

        internal PredicateExpression CurrentExpression
            => Predicate.Body.ElementAtOrDefault(_predicateExpressionIndex);

        internal IEnumerable<PredicateExpression> RemainingExpressions
            => Predicate.Body.Skip(_predicateExpressionIndex + 1);

        internal bool NextExpression()
        {
            _predicateExpressionIndex++;
            return _predicateExpressionIndex < Predicate.Body.Count;
        }

        internal IPredicateProvider PredicateProvider { get; }

        internal PredicateVariableLifeCycleContext VariableLifeCycleContext
            => _variableLifeCycleContexts.Peek();

        internal void BeginVariableLifeCycle(IEnumerable<PredicateParameterMapping> parameterMappings)
            => _variableLifeCycleContexts.Push(new PredicateVariableLifeCycleContext(parameterMappings));

        internal PredicateVariableLifeCycleContext EndVariableLifeCycle()
        {
            var oldVariableLifeCycleContext = _variableLifeCycleContexts.Pop();
            oldVariableLifeCycleContext.ClearVariables();

            if (_variableLifeCycleContexts.Count > 0)
            {
                var currentVariableLifeCycleContext = _variableLifeCycleContexts.Peek();
                foreach (var callParameter in oldVariableLifeCycleContext.CallParameterMapping.Values.Where(callParameter => callParameter.Name is object))
                    currentVariableLifeCycleContext.AddVariable(callParameter);

                var oldResultParameters = oldVariableLifeCycleContext.ResultParameterMapping.Values.Distinct();
                foreach (var oldResultParameter in oldResultParameters)
                {
                    var commonCallParameters = oldResultParameter.BoundParameters.Where(currentVariableLifeCycleContext.ResultParameterMapping.ContainsKey);
                    var firstCommonCallParameter = commonCallParameters.FirstOrDefault();
                    if (firstCommonCallParameter is object)
                    {
                        var currentResultParameter = currentVariableLifeCycleContext.ResultParameterMapping[firstCommonCallParameter];
                        if (firstCommonCallParameter is OutputParameter && oldResultParameter.IsBoundToValue)
                            currentResultParameter.BindValue(oldResultParameter.BoundValue);
                        foreach (var otherCommonCallParameter in commonCallParameters.Skip(1))
                        {
                            currentResultParameter.BindParameter(currentVariableLifeCycleContext.ResultParameterMapping[otherCommonCallParameter]);
                            currentVariableLifeCycleContext.ResultParameterMapping[otherCommonCallParameter] = currentResultParameter;
                        }
                    }
                }
            }

            return oldVariableLifeCycleContext;
        }

        internal void AddPredicateProvider(Func<CancellationToken, IAsyncEnumerable<Predicate>> predicateProvider)
            => _addAdditionalPredicateProviderCallback(new ProcessorPredicateProvider(predicateProvider));

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