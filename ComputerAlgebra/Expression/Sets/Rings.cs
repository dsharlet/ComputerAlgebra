using System.Collections.Generic;

namespace ComputerAlgebra
{
    public abstract class Ring : Set
    {
        private int order = 0;
        // Helper to determine ordering of rings.
        protected int Order { get { return order; } }

        protected Ring(int Order) { order = Order; }

        public override sealed IEnumerable<Atom> Atoms { get { yield break; } }

        public override int CompareTo(Expression R)
        {
            if (R is Ring RR)
                return Order.CompareTo(RR.Order);
            return base.CompareTo(R);
        }
        public override bool Equals(Expression E)
        {
            if (E is Ring R)
                return Order.Equals(R.Order);
            return base.Equals(E);
        }
        public override int GetHashCode() { return Order.GetHashCode(); }
    }

    public class Booleans : Ring
    {
        protected Booleans() : base(0) { }

        private static readonly Booleans instance = new Booleans();
        public static Booleans New() { return instance; }

        public override bool Contains(Expression x) { return x.IsTrue() || x.IsFalse(); }
    }

    public class Integers : Ring
    {
        protected Integers() : base(1) { }

        private static readonly Integers instance = new Integers();
        public static Integers New() { return instance; }

        public override bool Contains(Expression x) { return x.IsInteger(); }
    }

    public class Reals : Ring
    {
        protected Reals() : base(3) { }

        private static readonly Reals instance = new Reals();
        public static Reals New() { return instance; }

        public override bool Contains(Expression x) { return true; }
    }
}
