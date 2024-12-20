﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// List of equations and unknowns that supports row reduction and substitution of linear systems of equations. 
    /// </summary>
    public class SystemOfEquations : IEnumerable<LinearCombination>, IEnumerable
    {
        // Single equation (linear combination of terms).
        private class Equation : DefaultDictionary<Expression, Expression>
        {
            private void AddTerm(IEnumerable<Expression> B, Expression t)
            {
                foreach (Expression b in B)
                {
                    if (Product.TermsOf(t).Count(i => i.Equals(b)) == 1)
                    {
                        this[b] += Product.New(Product.TermsOf(t).Except(b));
                        return;
                    }
                }
                this[1] += t;
            }

            public Equation(Equal Eq, IEnumerable<Expression> Terms)
                : base(0)
            {
                Terms = Terms.AsList();

                Expression f = Eq.Left - Eq.Right;
                foreach (Expression i in Sum.TermsOf(f.Expand()))
                    AddTerm(Terms, i);
            }

            public Equation(IEnumerable<KeyValuePair<Expression, Expression>> Terms)
                : base(0)
            {
                foreach (KeyValuePair<Expression, Expression> i in Terms)
                    this[i.Key] = i.Value;
            }

            public Expression Solve(Expression x)
            {
                return Unary.Negate(Sum.New(this.Where(i => !i.Key.Equals(x)).Select(i => Product.New(i.Key, i.Value)))) / this[x];
            }

            public Expression Expression { get { return Sum.New(this.Select(i => Product.New(i.Key, i.Value))); } }

            public bool DependsOn(IEnumerable<Expression> x)
            {
                DependsOnVisitor v = new DependsOnVisitor(x);
                // It's faster to check the keys first.
                return
                    this.Any(i => v.Visit(i.Key)) ||
                    this.Any(i => v.Visit(i.Value));
            }
            public bool DependsOn(params Expression[] x) { return DependsOn(x.AsEnumerable()); }

            public IEnumerable<Expression> Unknowns { get { return this.Keys.Where(i => !(i is Constant)); } }

            public override string ToString()
            {
                return Expression.ToString() + " == 0";
            }
        }

        private List<Expression> unknowns;
        /// <summary>
        /// Enumerate the unknowns x in the system.
        /// </summary>
        public IEnumerable<Expression> Unknowns { get { return unknowns; } }

        private List<Equation> equations = new List<Equation>();
        /// <summary>
        /// Enumerate the equations in the system in the form F(x) == 0.
        /// </summary>
        public IEnumerable<LinearCombination> Equations { get { return equations.Select(i => LinearCombination.New(i)); } }

        private SystemOfEquations(List<Equation> Equations, List<Expression> Unknowns)
        {
            equations = Equations;
            unknowns = Unknowns;
        }

        /// <summary>
        /// Create a new system of equations with the given equations and unknowns.
        /// </summary>
        /// <param name="Equations"></param>
        /// <param name="Unknowns"></param>
        public SystemOfEquations(IEnumerable<Equal> Equations, IEnumerable<Expression> Unknowns)
        {
            unknowns = Unknowns.ToList();
            AddRange(Equations);
        }

        public SystemOfEquations(IEnumerable<IEnumerable<KeyValuePair<Expression, Expression>>> Equations, IEnumerable<Expression> Unknowns)
        {
            unknowns = Unknowns.ToList();
            AddRange(Equations);
        }

        /// <summary>
        /// Create an empty system of equations for the unknowns.
        /// </summary>
        /// <param name="Unknowns"></param>
        public SystemOfEquations(IEnumerable<Expression> Unknowns)
        {
            unknowns = Unknowns.ToList();
        }

        public void Add(Equal Eq) { equations.Add(new Equation(Eq, unknowns)); }
        public void Add(IEnumerable<KeyValuePair<Expression, Expression>> Eq) { equations.Add(new Equation(Eq)); }
        public void AddRange(IEnumerable<Equal> Eqs) { equations.AddRange(Eqs.Select(i => new Equation(i, unknowns))); }
        public void AddRange(IEnumerable<IEnumerable<KeyValuePair<Expression, Expression>>> Eqs) { equations.AddRange(Eqs.Select(i => new Equation(i))); }

        // The first pivot cost function is the magnitude of the pivot.
        private Real PivotScore(int i, int j, IList<Expression> Columns, IEnumerable<Arrow> PivotConditions)
        {
            Expression ij = equations[i][Columns[j]];
            if (ij.EqualsZero())
                return Real.NegativeInfinity;

            // Select the larger pivot if this is a constant, using the PivotConditions.
            if (!(ij is Constant) && PivotConditions != null)
                ij = ij.Evaluate(PivotConditions);
            return (ij is Constant C) ? Real.Abs(C.Value) : 0;
        }

        // The second pivot cost function is the number of things already zero that don't need to be eliminated.
        private int PivotEliminationZeros(int row, int col, int pi, int pj, IList<Expression> Columns)
        {
            // A pivot is better if the row containing the pivot has more zeros.
            int zeros = Columns.Count - equations[pi].Count;

            // A pivot is better if the column containing the pivot has more zeros.
            for (int i = row + 1; i < equations.Count; ++i)
            {
                if (i == pi) continue;

                if (equations[i][Columns[pj]].EqualsZero())
                    zeros += 1;
            }

            return zeros;
        }

        // Find the best pivot using full or partial pivoting.
        private Tuple<int, int> Pivot(int row, int col, IList<Expression> Columns, bool FullPivoting, IEnumerable<Arrow> PivotConditions)
        {
            // If we are full pivoting, we can consider any column after j.
            int maxj = FullPivoting ? Columns.Count - 1 : col;

            int besti = -1;
            int bestj = -1;
            Real score = Real.NegativeInfinity;
            // To avoid computing the number of zeros unnecessarily, -1 means uncomputed.
            int zeros = -1;

            for (int i = row; i < equations.Count; ++i)
            {
                for (int j = col; j <= maxj; ++j)
                {
                    // Check if we found a bigger pivot first.
                    Real s = PivotScore(i, j, Columns, PivotConditions);
                    if (s > score)
                    {
                        score = s;
                        // We don't know the tie-breaker cost yet.
                        zeros = -1;
                        besti = i;
                        bestj = j;
                    } 
                    else if (s > score / 8 && besti >= 0)
                    {
                        // The pivots are close enough. If another pivot has fewer non-zero eliminations, we should use that instead.
                        if (zeros == -1)
                            zeros = PivotEliminationZeros(row, col, besti, bestj, Columns);
                        int e = PivotEliminationZeros(row, col, i, j, Columns);
                        if (e > zeros)
                        {
                            // Don't update the score, so we don't repeatedly decay the score.
                            zeros = e;
                            besti = i;
                            bestj = j;
                        }
                    }
                }
            }
            return new Tuple<int, int>(besti, bestj);
        }

        // Swap rows i1 and i2.
        private void Swap(int i1, int i2)
        {
            Equation t = equations[i1];
            equations[i1] = equations[i2];
            equations[i2] = t;
        }

        // Swap columns j1 and j2.
        private static void Swap(IList<Expression> x, int j1, int j2)
        {
            Expression t = x[j1];
            x[j1] = x[j2];
            x[j2] = t;
        }

        // Eliminate the pivot position from row t using row s.
        private void Eliminate(int s, int t, Expression p, IEnumerable<Expression> Columns)
        {
            Equation T = equations[t];
            if (T[p].EqualsZero())
                return;

            Equation S = equations[s];

            // This is a pretty hot path, so avoid unnecessary evaluations.
            Expression scale = Product.New(-1, T[p], Binary.Power(S[p], -1));
            foreach (Expression j in Columns)
                if (!S[j].EqualsZero())
                    T[j] += Product.New(S[j], scale);
            T[p] = 0;
        }

        private void RowReduce(IList<Expression> Columns, bool FullPivoting, IEnumerable<Arrow> PivotConditions)
        {
            int row = 0;
            List<Expression> elim = unknowns.Append(1).ToList();
            for (int _j = 0; _j < Columns.Count; ++_j)
            {
                // Find the best pivot to use.
                Tuple<int, int> pivot = Pivot(row, _j, Columns, FullPivoting, PivotConditions);
                if (pivot.Item1 != -1)
                {
                    // Found a pivot, swap the rows and eliminate the remaining rows.
                    if (pivot.Item1 != row)
                        Swap(pivot.Item1, row);
                    if (_j != pivot.Item2)
                        Swap(Columns, _j, pivot.Item2);

                    Expression j = Columns[_j];
                    elim = elim.Except(j).ToList();
                    for (int i = row + 1; i < equations.Count; ++i)
                        Eliminate(row, i, j, elim);

                    ++row;
                }
            }
            equations.RemoveAll(i => i.Empty());
        }

        /// <summary>
        /// Row reduce the system in terms of the given columns with partial pivoting.
        /// </summary>
        /// <param name="Columns">Columns to perform row reduction on.</param>
        /// <param name="PivotConditions">Substitutions to use when considering an element as a pivot.</param>
        public void RowReduce(IEnumerable<Expression> Columns, IEnumerable<Arrow> PivotConditions = null)
        {
            RowReduce(Columns.AsList(), false, PivotConditions);
        }

        /// <summary>
        /// Row reduce the system in terms of the given columns with full pivoting. The Columns will be reordered according to a full pivot solution.
        /// </summary>
        /// <param name="Columns">Columns to perform row reduction on.</param>
        /// <param name="PivotConditions">Substitutions to use when considering an element as a pivot.</param>
        public void RowReduce(IList<Expression> Columns, IEnumerable<Arrow> PivotConditions = null)
        {
            RowReduce(Columns, true, PivotConditions);
        }

        /// <summary>
        /// Row reduce the system. The unknowns will be reorderd to reflect the full pivot solution.
        /// </summary>
        /// <param name="PivotConditions">Substitutions to use when considering an element as a pivot.</param>
        public void RowReduce(IEnumerable<Arrow> PivotConditions = null) { RowReduce(unknowns, PivotConditions); }

        private void BackSubstitute(IList<Expression> x)
        {
            List<Expression> elim = unknowns.Append(1).ToList();
            for (int i = Math.Min(x.Count, equations.Count) - 1, _j = x.Count - 1; _j >= 0; --_j)
            {
                Expression j = x[_j];
                elim = elim.Except(j).ToList();

                // While we still haven't reached a pivot row...
                while (i >= 0 && x.Take(_j).All(j2 => equations[i][j2].EqualsZero()))
                {
                    if (!equations[i][j].EqualsZero())
                    {
                        for (int i2 = i - 1; i2 >= 0; --i2)
                            Eliminate(i, i2, j, elim);
                        break;
                    }

                    --i;
                }
            }
            equations.RemoveAll(i => i.Empty());
        }

        /// <summary>
        /// Back-substitute the solutions for the given columns.
        /// </summary>
        /// <param name="Columns"></param>
        public void BackSubstitute(IEnumerable<Expression> Columns) { BackSubstitute(Columns.AsList()); }
        /// <summary>
        /// Back-substitute the solutions for all of the columns in the system.
        /// </summary>
        public void BackSubstitute() { BackSubstitute(unknowns); }

        private bool Solve(Expression x_i, IList<Expression> x, List<Arrow> fwd, List<Arrow> back)
        {
            foreach (Equation eq in equations.Reverse<Equation>())
            {
                if (eq[x_i].EqualsZero()) continue;

                bool allowFwd = !equations.Except(eq).Any(i => i.DependsOn(x_i));

                Expression s = eq.Solve(x_i);
                allowFwd = allowFwd && !s.DependsOn(x_i);
                bool allowBack = back != null && !s.DependsOn(unknowns);

                if (allowFwd)
                    fwd.Add(Arrow.New(x_i, s));
                else if (allowBack)
                    back.Add(Arrow.New(x_i, s));
                else
                    continue;

                // Remove the equation and unknown.
                equations.Remove(eq);
                x.Remove(x_i);
                if (!ReferenceEquals(x, unknowns))
                    unknowns.Remove(x_i);
                return true;
            }
            return false;
        }

        private bool SolveOne(IList<Expression> x, List<Arrow> fwd, List<Arrow> back)
        {
            foreach (Expression x_i in x)
                if (Solve(x_i, x, fwd, back))
                    return true;
            return false;
        }

        /// <summary>
        /// Solve the system for the given variables. The system must already be in row-echelon form, optionally with back substitution.
        /// 
        /// This method removes the equations and unknowns from the system as they are solved.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The solutions for the unknowns that were successfully solved. If the system was not back substituted, solutions may
        /// depend on previous solutions in the list.</returns>
        public List<Arrow> Solve(IEnumerable<Expression> x) {
            List<Arrow> fwd = new List<Arrow>();
            PartialSolve(x.AsList(), fwd, null);
            return fwd;
        }
        public List<Arrow> Solve() { return Solve(unknowns); }

        /// <summary>
        /// Solve the system for the given variables. The system must already be in row-echelon form, optionally with back substitution.
        /// 
        /// This method removes the equations and unknowns from the system as they are solved.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="fwd"></param>
        /// <param name="back"></param>
        public void PartialSolve(IList<Expression> x, List<Arrow> fwd, List<Arrow> back)
        {
            fwd.Reverse();
            while (!x.Empty())
            {
                if (!SolveOne(x, fwd, back)) break;
            }
            fwd.Reverse();
        }
        public void PartialSolve(List<Arrow> fwd, List<Arrow> back) { PartialSolve(unknowns, fwd, back); }

        /// <summary>
        /// Find independent systems of equations within this system.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SystemOfEquations> Partition()
        {
            List<Expression> x = new List<Expression>();
            List<Equation> eqs = new List<Equation>();
            while (unknowns.Any())
            {
                x.Add(unknowns.Last());
                unknowns.RemoveAt(unknowns.Count - 1);

                do
                {
                    eqs.AddRange(equations.Where(i => i.DependsOn(x)));
                    equations.RemoveAll(i => eqs.Contains(i));

                    x.AddRange(unknowns.Where(i => eqs.Any(j => j.DependsOn(i))));
                    unknowns.RemoveAll(i => x.Contains(i));
                } while (equations.Any(i => i.DependsOn(x)));

                yield return new SystemOfEquations(eqs, x);
                x.Clear();
                eqs.Clear();
            }
        }

        // IEnumerable<LinearCombination>
        public IEnumerator<LinearCombination> GetEnumerator() { return Equations.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}
