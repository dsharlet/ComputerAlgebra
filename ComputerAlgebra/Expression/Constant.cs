using System;

namespace ComputerAlgebra
{
    public class Constant : Atom
    {
        protected Real x;
        public Real Value { get { return x; } }

        protected Constant(Real x) { this.x = x; }

        private static readonly Constant NegativeOne = new Constant(-1);
        private static readonly Constant Zero = new Constant(0);
        private static readonly Constant One = new Constant(1);

        public static Constant New(int x)
        {
            if (x == -1) return NegativeOne;
            if (x == 0) return Zero;
            if (x == 1) return One;
            return new Constant(x);
        }
        public static Constant New(double x) { return new Constant(x); }
        public static Constant New(decimal x) { return new Constant(x); }
        public static Constant New(Real x) { return new Constant(x); }
        public static Constant New(bool x) { return x ? One : Zero; }
        public static Expression New(object x)
        {
            if (x is int i) return New(i);
            if (x is double dbl) return New(dbl);
            if (x is decimal d) return New(d);
            if (x is bool b) return New(b);
            if (x is Real r) return New(r);
            throw new InvalidCastException();
        }

        public override bool IsInteger() { return x.IsInteger(); }
        public override bool EqualsZero() { return x.EqualsZero(); }
        public override bool EqualsOne() { return x.EqualsOne(); }
        public override bool IsFalse() { return EqualsZero(); }
        public override bool IsTrue() { return !EqualsZero(); }

        public static implicit operator Real(Constant x) { return x.x; }

        public static implicit operator Constant(Real x) { return Constant.New(x); }
        public static implicit operator Constant(BigRational x) { return Constant.New(x); }
        public static implicit operator Constant(decimal x) { return Constant.New(x); }
        public static implicit operator Constant(double x) { return Constant.New(x); }
        public static implicit operator Constant(int x) { return Constant.New(x); }

        // Atom.
        protected override int TypeRank { get { return 3; } }

        // object interface.
        public override int GetHashCode() { return x.GetHashCode(); }

        // Note that this is *not* an arithmetic comparison, it is a canonicalization ordering.
        public override int CompareTo(Expression R)
        {
            if (R is Constant RC)
                return Real.Abs(RC.Value).CompareTo(Real.Abs(Value));
            return base.CompareTo(R);
        }
        public override bool Equals(Expression E)
        {
            if (E is Constant C)
                return Value.Equals(C.Value);
            return base.Equals(E);
        }
    }
}
