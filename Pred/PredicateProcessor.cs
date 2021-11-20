using System;

namespace Pred
{
    public class PredicateProcessor
    {
        public void Process(string predicateName)
        {
            throw new ArgumentException($"'{predicateName}' predicate does not exist.", nameof(predicateName));
        }
    }
}