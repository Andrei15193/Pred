namespace Pred.Expressions
{
    public abstract class CheckPredicateExpression : PredicateExpression
    {
        protected CheckPredicateExpression()
        {
        }

        protected internal abstract bool Check(PredicateExpressionContext context);
    }
}