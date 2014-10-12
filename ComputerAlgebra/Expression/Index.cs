using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
            Debug.Assert(!ReferenceEquals(Target, null));
            target = Target; 
            indices = Indices; 
        }

        public static Index New(Expression Target, IEnumerable<Expression> Indices) { return new Index(Target, Indices.Buffer()); }
        public static Index New(Expression Target, params Expression[] Indices) { return new Index(Target, Indices); }

        public override bool Matches(Expression E, MatchContext Matched)
        {
            Index I = E as Index;
            if (!ReferenceEquals(I, null))
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
            Index I = E as Index;
            if (ReferenceEquals(I, null)) return base.Equals(E);

            return target.Equals(I.Target) && indices.SequenceEqual(I.Indices);
        }
        public override int GetHashCode() { return target.GetHashCode() ^ indices.OrderedHashCode(); }

        // Atom interface.
        protected override int TypeRank { get { return 5; } }
        public override int CompareTo(Expression R)
        {
            Index RF = R as Index;
            if (!ReferenceEquals(RF, null))
                return LexicalCompareTo(
                    () => target.CompareTo(RF.Target),
                    () => indices.LexicalCompareTo(RF.Indices));

            return base.CompareTo(R);
        }
    }
}