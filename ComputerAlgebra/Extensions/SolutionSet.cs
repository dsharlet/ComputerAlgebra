using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a set of solutions for a system of equations.
    /// </summary>
    public abstract class SolutionSet
    {
        /// <summary>
        /// Enumerate the unknowns solved by this solution set.
        /// </summary>
        public abstract IEnumerable<Expression> Unknowns { get; }

        /// <summary>
        /// Check if any of the solutions of this system depend on x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public abstract bool DependsOn(Expression x);
    }

    /// <summary>
    /// A simple linear system solution set. The system directly gives solutions.
    /// </summary>
    public class LinearSolutions : SolutionSet
    {
        private List<Arrow> solutions;
        /// <summary>
        /// Enumerate the solutions to this system.
        /// </summary>
        public IEnumerable<Arrow> Solutions { get { return solutions; } }

        public override IEnumerable<Expression> Unknowns { get { return solutions.Select(i => i.Left); } }

        public LinearSolutions(IEnumerable<Arrow> Solutions) { solutions = Solutions.ToList(); }

        public override bool DependsOn(Expression x) { return solutions.Any(i => i.Right.DependsOn(x)); }
    }

    /// <summary>
    /// A solution set described by an iteration of newton's method, partially solved.
    /// </summary>
    public class NewtonIteration : SolutionSet
    {
        private IEnumerable<Arrow> knowns;
        /// <summary>
        /// Enumerate the solved Newton deltas.
        /// </summary>
        public IEnumerable<Arrow> KnownDeltas { get { return knowns; } }

        private IEnumerable<Expression> unknowns;
        /// <summary>
        /// Enumerate the unkonwn Newton deltas described by Equations.
        /// </summary>
        public IEnumerable<Expression> UnknownDeltas { get { return unknowns; } }

        /// <summary>
        /// Enumerate both the solved and unsolved the Newton update deltas in this solution set.
        /// </summary>
        public IEnumerable<Expression> Deltas { get { return knowns != null ? knowns.Select(i => i.Left).Concat(unknowns) : unknowns; } }

        private IEnumerable<LinearCombination> equations;
        /// <summary>
        /// Enumerate the equations describing the unsolved part of this system.
        /// </summary>
        public IEnumerable<LinearCombination> Equations { get { return equations; } }

        private IEnumerable<Arrow> guesses;
        /// <summary>
        /// Initial guesses for the first iteration.
        /// </summary>
        public IEnumerable<Arrow> Guesses { get { return guesses; } }

        public override IEnumerable<Expression> Unknowns { get { return Deltas.Select(i => DeltaOf(i)); } }

        public NewtonIteration(IEnumerable<LinearCombination> Equations, IEnumerable<Expression> Deltas, IEnumerable<Arrow> Guesses)
        {
            knowns = null;
            equations = Equations.Buffer();
            unknowns = Deltas.Buffer();
            guesses = Guesses.Buffer();
        }

        public NewtonIteration(IEnumerable<Arrow> KnownDeltas, IEnumerable<LinearCombination> Equations, IEnumerable<Expression> UnknownDeltas, IEnumerable<Arrow> Guesses)
        {
            knowns = KnownDeltas.Buffer();
            equations = Equations.Buffer();
            unknowns = UnknownDeltas.Buffer();
            guesses = Guesses.Buffer();
        }

        public override bool DependsOn(Expression x)
        {
            if (knowns != null && knowns.Any(i => i.Right.DependsOn(x)))
                return true;
            if (guesses != null && guesses.Any(i => i.Right.DependsOn(x)))
                return true;
            return equations.Any(i => i.DependsOn(x));
        }

        private static Function d = UnknownFunction.New("d", Variable.New("x"));
        public static Expression Delta(Expression x) { return Call.New(d, x); }
        public static Expression DeltaOf(Expression x)
        {
            Call c = (Call)x;
            if (c.Target == d)
                return c.Arguments.First();
            throw new InvalidCastException("Expression is not a Newton Delta");
        }
    }
}
