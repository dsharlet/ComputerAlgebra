using System;
using System.Collections;
using System.Collections.Generic;
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
                SearchVisitor v = new SearchVisitor(x);
                // It's faster to check the keys first.
                return
                    this.Any(i => v.Visit(i.Key) == null) ||
                    this.Any(i => v.Visit(i.Value) == null);
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

        // Find the best pivot in column j.
        private KeyValuePair<int, Real> PartialPivot(int i1, int i2, Expression j)
        {
            int row = -1;
            Real max = -1;
            for (int i = i1; i < i2; ++i)
            {
                Expression ij = equations[i][j];
                if (!ij.EqualsZero())
                {
                    // If we don't a pivot yet, just grab this one.
                    if (row == -1)
                        row = i;

                    // Select the larger pivot if this is a constant.
                    if (ij is Constant && Real.Abs((Real)ij) > max)
                    {
                        row = i;
                        max = Real.Abs((Real)ij);
                    }
                }
            }

            return new KeyValuePair<int, Real>(row, max);
        }

        // Find the best pivot from all of the columns. This is important to avoid non-constant terms from blowing up the system.
        private KeyValuePair<int, int> FullPivot(int i1, int i2, int j1, IList<Expression> Columns)
        {
            int row = -1;
            Real max = -2;
            int col = -1;
            int elims = i2 - i1 + 1;
            for (int j = j1; j < Columns.Count; ++j)
            {
                KeyValuePair<int, Real> partial = PartialPivot(i1, i2, Columns[j]);
                if (partial.Key == -1)
                    continue;

                if (partial.Value > max)
                {
                    bool constant = partial.Value > 0;
                    // If the best pivot is a non-constant term, choose the non-constant term with the fewest eliminations.
                    int es = !constant ? Enumerable.Range(i1, i2 - i1).Count(i => !equations[i][Columns[j]].EqualsZero()) : 0;
                    if (constant || es < elims)
                    {
                        row = partial.Key;
                        max = partial.Value;
                        col = j;
                        elims = es;
                    }
                }
            }

            return new KeyValuePair<int, int>(row, col);
        }

        // Find the best pivot using full or partial pivoting.
        private KeyValuePair<int, int> Pivot(int i1, int i2, int j, IList<Expression> Columns, bool FullPivoting)
        {
            if (FullPivoting)
                return FullPivot(i1, i2, j, Columns);
            else
                return new KeyValuePair<int, int>(PartialPivot(i1, i2, Columns[j]).Key, j);
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

        private void RowReduce(int i1, IList<Expression> Columns, bool FullPivoting)
        {
            List<Expression> elim = unknowns.Append(1).ToList();
            for (int i2 = equations.Count, _j = 0; _j < Columns.Count; ++_j)
            {
                // Find the best pivot to use.
                KeyValuePair<int, int> pivot = Pivot(i1, i2, _j, Columns, FullPivoting);
                if (pivot.Key != -1)
                {
                    // Found a pivot, swap the rows and eliminate the remaining rows.
                    if (pivot.Key != i1)
                        Swap(pivot.Key, i1);
                    if (_j != pivot.Value)
                        Swap(Columns, _j, pivot.Value);

                    Expression j = Columns[_j];
                    elim = elim.Except(j).ToList();
                    for (int i = i1 + 1; i < i2; ++i)
                        Eliminate(i1, i, j, elim);

                    ++i1;
                }
            }
        }

        /// <summary>
        /// Row reduce the system in terms of the given columns with partial pivoting.
        /// </summary>
        /// <param name="Columns"></param>
        public void RowReduce(IEnumerable<Expression> Columns) { RowReduce(0, Columns.AsList(), false); }
        /// <summary>
        /// Row reduce the system in terms of the given columns with full pivoting. The Columns will be reordered according to a full pivot solution.
        /// </summary>
        /// <param name="Columns"></param>
        public void RowReduce(IList<Expression> Columns) { RowReduce(0, Columns, true); }
        /// <summary>
        /// Row reduce the system. The unknowns will be reorderd to reflect the full pivot solution.
        /// </summary>
        public void RowReduce() { RowReduce(unknowns); }

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

        private List<Arrow> Solve(IList<Expression> x)
        {
            List<Arrow> solutions = new List<Arrow>();

            for (int i = Math.Min(x.Count, equations.Count) - 1, _j = x.Count - 1; _j >= 0; --_j)
            {
                Expression j = x[_j];

                // While we still haven't reached a pivot row...
                while (i >= 0 && x.Take(_j).All(j2 => equations[i][j2].EqualsZero()))
                {
                    if (!equations[i][j].EqualsZero())
                    {
                        // Solve this row for the pivot.
                        Expression s = equations[i].Solve(j);
                        if (!s.DependsOn(x))
                        {
                            solutions.Add(Arrow.New(j, s));

                            // Remove the equation and unknown.
                            equations.RemoveAt(i--);
                            x.RemoveAt(_j);
                            if (!ReferenceEquals(x, unknowns))
                                unknowns.Remove(j);
                        }
                        break;
                    }
                    else
                    {
                        --i;
                    }
                }
            }

            return solutions;
        }

        /// <summary>
        /// Solve the system for the given variables. The system must already be in row-echelon form, optionally with back substitution.
        /// 
        /// This method removes the equations and unknowns from the system as they are solved.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public List<Arrow> Solve(IEnumerable<Expression> x) { return Solve(x.AsList()); }
        /// <summary>
        /// Solve the system for all of the system variables. The system must already be in row-echelon form, optionally with back substitution.
        /// 
        /// This method removes the equations and unknowns from the system as they are solved.
        /// </summary>
        /// <returns></returns>
        public List<Arrow> Solve() { return Solve(unknowns); }

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
