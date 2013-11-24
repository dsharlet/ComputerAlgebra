using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
                if (t.DependsOn(B))
                {
                    foreach (Expression b in B)
                    {
                        Expression Tb = t / b;
                        if (!Tb.DependsOn(B))
                        {
                            this[b] += Tb;
                            return;
                        }
                    }
                }
                this[1] += t;
            }

            public Equation(Equal Eq, IEnumerable<Expression> Terms)
                : base(0)
            {
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

        // Find the best pivot in the column j.
        private KeyValuePair<int, Real> PartialPivot(int i, Expression j)
        {
            int row = -1;
            Real max = 0;
            for (; i < equations.Count; ++i)
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
        private KeyValuePair<int, int> FullPivot(int i, int j, IList<Expression> Columns)
        {
            int row = -1;
            Real max = -1;
            int col = -1;
            for (; i < equations.Count; ++i)
            {
                for (int _j = j; _j < Columns.Count; ++_j)
                {
                    KeyValuePair<int, Real> partial = PartialPivot(i, Columns[_j]);

                    if ((partial.Key != -1 && partial.Value > max))
                    {
                        row = partial.Key;
                        max = partial.Value;
                        col = _j;
                    }
                }
            }

            return new KeyValuePair<int, int>(row, col);
        }

        // Find the best pivot using full or partial pivoting.
        private KeyValuePair<int, int> Pivot(int i, int j, IList<Expression> Columns, bool FullPivoting)
        {
            if (FullPivoting)
                return FullPivot(i, j, Columns);
            else
                return new KeyValuePair<int, int>(PartialPivot(i, Columns[j]).Key, j);
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
        private void Eliminate(int s, int t, Expression p)
        {
            if (equations[t][p].EqualsZero())
                return;

            Expression scale = -equations[t][p] / equations[s][p];
            foreach (Expression j in unknowns.Append(1))
                equations[t][j] += equations[s][j] * scale;

            // Verify that the pivot position in the target is zero.
            Debug.Assert(equations[t][p].EqualsZero());
        }

        private void RowReduce(int Row, IList<Expression> Columns, bool FullPivoting)
        {
            for (int _j = 0; _j < Columns.Count; ++_j)
            {
                // Find the best pivot to use.
                KeyValuePair<int, int> pivot = Pivot(Row, _j, Columns, FullPivoting);
                if (pivot.Key != -1)
                {
                    // Found a pivot, swap the rows and eliminate the remaining rows.
                    Expression j = Columns[pivot.Value];
                    if (pivot.Key != Row)
                        Swap(pivot.Key, Row);
                    if (_j != pivot.Value)
                        Swap(Columns, _j, pivot.Value);

                    for (int i = Row + 1; i < equations.Count; ++i)
                        Eliminate(Row, i, j);

                    ++Row;
                }
            }
        }

        /// <summary>
        /// Row reduce the system in terms of the given columns with partial pivoting.
        /// </summary>
        /// <param name="Columns"></param>
        public void RowReduce(IEnumerable<Expression> Columns) { RowReduce(0, Columns.ToList(), false); }
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
            int i = Math.Min(x.Count, equations.Count) - 1;
            for (int _j = x.Count - 1; _j >= 0; --_j)
            {
                Expression j = x[_j];

                // While we still haven't reached a pivot row...
                while (i >= 0 && Enumerable.Range(0, _j).All(j2 => equations[i][x[j2]].EqualsZero()))
                {
                    if (!equations[i][j].EqualsZero())
                    {
                        for (int i2 = i - 1; i2 >= 0; --i2)
                            Eliminate(i, i2, j);
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

            int i = Math.Min(x.Count, equations.Count) - 1;
            for (int _j = x.Count - 1; _j >= 0; --_j)
            {
                Expression j = x[_j];

                // While we still haven't reached a pivot row...
                while (i >= 0 && Enumerable.Range(0, _j).All(j2 => equations[i][x[j2]].EqualsZero()))
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

        // IEnumerable<LinearCombination>
        public IEnumerator<LinearCombination> GetEnumerator() { return Equations.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}
