using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Indexing operator expression.
    /// </summary>
    public class Index : Atom
    {
        protected Expression target;
        public Expression Target { get { return target; } }

        protected IEnumerable<Expression> indices;
        public IEnumerable<Expression> Indices { get { return indices; } }

        protected Index(Expression Target, IEnumerable<Expression> Indices)
        {
            Debug.Assert(!(Target is null));
            target = Target;
            indices = Indices;
        }

        public static Index New(Expression Target, IEnumerable<Expression> Indices) { return new Index(Target, Indices.Buffer()); }
        public static Index New(Expression Target, params Expression[] Indices) { return new Index(Target, Indices); }

        public override bool Matches(Expression E, MatchContext Matched)
        {
            if (E is Index I)
            {
                if (Indices.Count() != I.Indices.Count())
                    return false;

                return Matched.TryMatch(() =>
                    target.Matches(I.Target, Matched) &&
                    indices.Zip(I.Indices, (p, e) => p.Matches(e, Matched)).All());
            }

            return false;
        }

        // object interface.
        public override bool Equals(Expression E)
        {
            if (E is Index I)
                return target.Equals(I.Target) && indices.SequenceEqual(I.Indices);
        
            return base.Equals(E);
        }
        public override int GetHashCode() { return target.GetHashCode() ^ indices.OrderedHashCode(); }

        // Atom interface.
        protected override int TypeRank { get { return 5; } }
        public override int CompareTo(Expression R)
        {
            if (R is Index RF)
                return LexicalCompareTo(
                    () => target.CompareTo(RF.Target),
                    () => indices.LexicalCompareTo(RF.Indices));

            return base.CompareTo(R);
        }
    }
}