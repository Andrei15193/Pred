using System;

namespace Pred
{
    public class Predicate
    {
        public Predicate(string name)
            => Name = name ?? throw new ArgumentNullException(nameof(name));

        public string Name { get; }
    }
}