using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Extensions for solving equations.
    /// </summary>
    public static class SolveExtension
    {
        /// <summary>
        /// Solve a linear equation or system of linear equations.
        /// </summary>
        /// <param name="Equations">Equation or set of equations to solve.</param>
        /// <param name="For">Variable of set of variables to solve for.</param>
        /// <returns>The solved values of x, including non-independent solutions.</returns>
        public static List<Arrow> Solve(this IEnumerable<Equal> Equations, IEnumerable<Expression> For)
        {
            SystemOfEquations S = new SystemOfEquations(Equations, For);
            S.RowReduce();
            S.BackSubstitute();
            return S.Solve();
        }

        /// <summary>
        /// Solve a linear equation or system of linear equations.
        /// </summary>
        /// <param name="Equations">Equation or set of equations to solve.</param>
        /// <param name="For">Variable of set of variables to solve for.</param>
        /// <returns>The solved values of x, including non-independent solutions.</returns>
        public static List<Arrow> Solve(this IEnumerable<Equal> Equations, params Expression[] For) { return Equations.Solve(For.AsEnumerable()); }


        /// <summary>
        /// Partially solve a linear equation or system of linear equations. Back substitution is not performed.
        /// </summary>
        /// <param name="Equations">Equation or set of equations to solve.</param>
        /// <param name="For">Variable of set of variables to solve for.</param>
        /// <returns>The solved values of x, including non-independent solutions.</returns>
        public static List<Arrow> PartialSolve(this IEnumerable<Equal> Equations, IEnumerable<Expression> For)
        {
            SystemOfEquations S = new SystemOfEquations(Equations, For);
            S.RowReduce();
            return S.Solve();
        }

        /// <summary>
        /// Partially solve a linear equation or system of linear equations. Back substitution is not performed.
        /// </summary>
        /// <param name="Equations">Equation or set of equations to solve.</param>
        /// <param name="For">Variable of set of variables to solve for.</param>
        /// <returns>The solved values of x, including non-independent solutions.</returns>
        public static List<Arrow> PartialSolve(this IEnumerable<Equal> Equations, params Expression[] For) { return Equations.PartialSolve(For.AsEnumerable()); }
    }
}
