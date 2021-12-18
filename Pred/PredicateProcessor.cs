using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameter)
            => ProcessAsync(predicateName, parameter, CancellationToken.None);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, params CallParameter[] parameters)
            => ProcessAsync(predicateName, parameters, CancellationToken.None);

        public IAsyncEnumerable<PredicateProcessResult> ProcessAsync(string predicateName, IEnumerable<CallParameter> parameters, CancellationToken cancellationToken)
        {
            if (predicateName is null)
                throw new ArgumentNullException(nameof(predicateName));
            var parametersList = parameters?.ToArray();
            if (parameters is null || parametersList.Contains(null))
                throw new ArgumentException("Cannot be null or contain null parameters.", nameof(parameters));

            return _ProcessAsync(predicateName, parametersList, cancellationToken);
        }

        private async IAsyncEnumerable<PredicateProcessResult> _ProcessAsync(string predicateName, IReadOnlyList<CallParameter> callParameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pendingPredicateProviders = new Queue<ProcessorPredicateProvider>();
            pendingPredicateProviders.Enqueue(new ProcessorPredicateProvider(cancellationToken => _predicateProvider.GetPredicatesAsync(predicateName, cancellationToken)));

            do
            {
                var predicateProvider = pendingPredicateProviders.Dequeue();
                await foreach (var predicate in predicateProvider.GetPredicatesAsync(cancellationToken).WithCancellation(cancellationToken))
                    if (CallParameter.AreParametersMatching(callParameters, predicate.Parameters))
                    {
                        Debug.WriteLine($"{predicate.Name}({string.Join(", ", callParameters.Select(callParameter => callParameter.Name))})");
                        Debug.Indent();

                        var context = new PredicateProcessorContext(predicate, _predicateProvider, pendingPredicateProviders.Enqueue);

                        context.BeginVariableLifeCycle(predicate.Parameters.Select((parameter, parameterIndex) => new PredicateParameterMapping(parameter, callParameters[parameterIndex])));

                        var visitor = new PredicateProcessorVisitor(context);
                        while (visitor.IsExpressionTrue && context.NextExpression())
                            context.CurrentExpression.Accept(visitor);

                        Debug.Unindent();
                        Debug.WriteLine(visitor.IsExpressionTrue ? "[complete]" : "[end]");

                        if (visitor.IsExpressionTrue)
                        {
                            var variableLifeCycleContext = context.EndVariableLifeCycle();
                            yield return new PredicateProcessResult(callParameters.Select(callParameter => new ResultParameterMapping(callParameter, variableLifeCycleContext.ResultParameterMapping[callParameter])));
                        }
                    }
            } while (pendingPredicateProviders.Count > 0);
        }
    }
}