using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ComputerAlgebra
{
    /// <summary>
    /// An Expression class that has Atoms = { this }. Atom classes will always compare to other atoms successfully.
    /// </summary>
    public abstract class Atom : Expression
    {
        protected virtual int TypeRank { get { return 3; } }

        public override sealed IEnumerable<Atom> Atoms { get { yield return this; } }

        // object
        public override abstract int GetHashCode();
        public override int CompareTo(Expression R)
        {
            Atom RA = R as Atom;
            if (!ReferenceEquals(RA, null))
                return TypeRank.CompareTo(RA.TypeRank);

            return base.CompareTo(R);
        }
    }

    /// <summary>
    /// Atom with a name.
    /// </summary>
    public class NamedAtom : Atom
    {
        private ulong cmp;

        private string name;
        public string Name { get { return name; } }

        protected NamedAtom(string Name) 
        { 
            name = Name;

            // Put the first few characters in an integer for fast comparisons.
            cmp = 0;
            int length = Marshal.SizeOf(cmp) / 2;
            for (int i = 0; i < Math.Min(name.Length, length); ++i)
                cmp |= (ulong)name[i] << ((length - i - 1) * 16);
        }

        public override int GetHashCode() { return name.GetHashCode(); }
        public override bool Equals(Expression E)
        {
            if (ReferenceEquals(E, null) || GetType() != E.GetType()) return base.Equals(E);
            return Equals(name, ((NamedAtom)E).name);
        }
        public override int CompareTo(Expression R)
        {
            NamedAtom RA = R as NamedAtom;
            if (!ReferenceEquals(RA, null))
            {
                int c = TypeRank.CompareTo(RA.TypeRank);
                if (c != 0) return c;

                // Try comparing the first 4 chars of the name.
                c = cmp.CompareTo(RA.cmp);
                if (c != 0)
                    return c;

                // First 4 chars match, need to use the full compare.
                return String.Compare(name, RA.name, StringComparison.Ordinal);
            }

            return base.CompareTo(R);
        }
    }
}
