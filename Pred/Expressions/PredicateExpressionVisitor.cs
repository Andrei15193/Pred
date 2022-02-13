namespace Pred.Expressions
{
    public abstract class PredicateExpressionVisitor
    {
        internal protected abstract void VisitConstantExpression(ConstantPredicateExpression constantExpression);

        internal protected abstract void VisitBindOrCheckExpression(BindOrCheckPredicateExpression bindOrCheckExpression);

        internal protected abstract void VisitParameterExpression(ParameterPredicateExpression parameterExpression);

        internal protected abstract void VisitCallExpression(CallPredicateExpression callExpression);

        internal protected abstract void VisitMapExpression(MapPredicateExpression mapExpression);

        internal protected abstract void VisitCheckExpression(CheckPredicateExpression checkExpression);

        internal protected abstract void VisitActionExpression(ActionPredicateExpression actionExpression);

        internal virtual void VisitBeginVariableLifeCycleExpression(BeginVariableLifeCyclePredicateExpression beginVariableLifeCycleExpression)
        {
        }

        internal virtual void VisitEndVariableLifeCycleExpression(EndVariableLifeCyclePredicateExpression endVariableLifeCycleExpression)
        {
        }
    }
}