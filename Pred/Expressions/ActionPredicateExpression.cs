namespace Pred.Expressions
{
    public abstract class ActionPredicateExpression : PredicateExpression
    {
        protected ActionPredicateExpression()
        {
        }

        protected internal abstract void Process(PredicateExpressionContext context);

        public sealed override void Accept(PredicateExpressionVisitor visitor)
            => visitor.VisitActionExpression(this);
    }
}