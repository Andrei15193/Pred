namespace Pred.Expressions
{
    public abstract class PredicateExpressionVisitor
    {
        internal protected abstract void Visit(ConstantPredicateExpression constantExpression);

        internal protected abstract void Visit(BindOrCheckPredicateExpression bindOrCheckExpression);

        internal protected abstract void Visit(ParameterPredicateExpression parameterExpression);

        internal protected abstract void Visit(CallPredicateExpression callExpression);

        internal protected abstract void Visit(MapPredicateExpression mapExpression);

        internal protected abstract void Visit(CheckPredicateExpression checkExpression);

        internal protected abstract void Visit(ActionPredicateExpression actionExpression);

        internal abstract void Visit(BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression);

        internal abstract void Visit(EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression);
    }
}