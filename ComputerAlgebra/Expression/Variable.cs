using System;

namespace ComputerAlgebra
{
    /// <summary>
    /// Variable expression.
    /// </summary>
    public class Variable : NamedAtom
    {
        private Set ring;
        /// <summary>
        /// Identifies the ring this variable is a member of.
        /// </summary>
        public Set Ring { get { return ring; } }

        protected Variable(string Name, Set Ring) : base(Name) { ring = Ring; }

        /// <summary>
        /// Create a new variable.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static Variable New(string Name, Set Ring) { return new Variable(Name, Ring); }
        /// <summary>
        /// Create a new variable.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static Variable New(string Name) { return new Variable(Name, Reals.New()); }

        public override bool Matches(Expression E, MatchContext Matched)
        {
            return (Ring is null || Ring.Contains(E)) && Matched.Matches(this, E);
        }

        protected override int TypeRank { get { return 1; } }
    }

    /// <summary>
    /// Variable the checks a predicate before matching an expression.
    /// </summary>
    class PatternVariable : Variable
    {
        protected Func<Expression, bool> condition;

        private PatternVariable(string Name, Set Ring, Func<Expression, bool> Condition) : base(Name, Ring) { condition = Condition; }

        /// <summary>
        /// Create a new variable with a condition callback for matching.
        /// </summary>
        /// <param name="Name"></param>
        /// <param Name="Ring"></param>
        /// <param name="Condition">A function that should return true if the variable is allowed to match the given Expression.</param>
        /// <returns></returns>
        public static PatternVariable New(string Name, Set Ring, Func<Expression, bool> Condition) { return new PatternVariable(Name, Ring, Condition); }
        public static PatternVariable New(string Name, Func<Expression, bool> Condition) { return new PatternVariable(Name, Reals.New(), Condition); }

        public override bool Matches(Expression E, MatchContext Matched)
        {
            if (condition(E))
                return base.Matches(E, Matched);
            else
                return false;
        }
    }
}
