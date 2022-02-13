namespace Pred.Expressions
{
    internal sealed class EndVariableLifeCyclePredicateExpression : PredicateExpression
    {
        public sealed override void Accept(PredicateExpressionVisitor visitor)
            => visitor.VisitEndVariableLifeCycleExpression(this);
    }
}