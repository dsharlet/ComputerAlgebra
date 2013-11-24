using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    /// <summary>
    /// Decompose an expression into a linear combination of basis variables.
    /// </summary>
    public class LinearCombination : Sum, IEnumerable<KeyValuePair<Expression, Expression>>, IEnumerable
    {
        private DefaultDictionary<Expression, Expression> terms = new DefaultDictionary<Expression, Expression>(0);

        public override IEnumerable<Expression> Terms
        {
            get
            {
                foreach (KeyValuePair<Expression, Expression> i in terms)
                {
                    if (i.Value.EqualsOne())
                        yield return i.Key;
                    else if (i.Key.EqualsOne())
                        yield return i.Value;
                    else
                        yield return Product.New(i.Key, i.Value);
                }
            }
        }

        private void AddTerm(IEnumerable<Expression> B, Expression t)
        {
            if (t.DependsOn(B))
            {
                foreach (Expression b in B)
                {
                    Expression Tb = t / b;
                    if (!Tb.DependsOn(B))
                    {
                        terms[b] += Tb;
                        return;
                    }
                }
            }
            terms[1] += t;
        }

        /// <summary>
        /// Basis variables of this linear combination.
        /// </summary>
        public IEnumerable<Expression> Basis { get { return terms.Keys; } }

        /// <summary>
        /// Coefficients of this linear combination.
        /// </summary>
        /// <param name="b">Basis variable to access the coefficient for.</param>
        /// <returns></returns>
        public Expression this[Expression b] { get { return terms[b]; } }

        private LinearCombination() { }
        private LinearCombination(IEnumerable<KeyValuePair<Expression, Expression>> Terms)
        {
            foreach (KeyValuePair<Expression, Expression> i in Terms)
                terms[i.Key] = i.Value;
        }

        /// <summary>
        /// Create a new linear combination expression equal to x.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static LinearCombination New(IEnumerable<Expression> A, Expression x)
        {
            LinearCombination ret = new LinearCombination();
            foreach (Expression t in Sum.TermsOf(x.Expand()))
                ret.AddTerm(A, t);
            return ret;
        }

        /// <summary>
        /// Createa  new linear combination with the given list of terms.
        /// </summary>
        /// <param name="Terms"></param>
        /// <returns></returns>
        public static LinearCombination New(IEnumerable<KeyValuePair<Expression, Expression>> Terms) { return new LinearCombination(Terms); }
        
        /// <summary>
        /// Solve the expression for the variable v.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Expression Solve(Expression v)
        {
            // Subtract independent terms.
            Expression R = Sum.New(terms.Where(x => !x.Key.Equals(v)).Select(x => x.Key * x.Value));
            // Divide coefficient from dependent term.
            return R / Unary.Negate(this[v]);
        }

        // IEnumerable<KeyValuePair<Expression, Expression>>
        public IEnumerator<KeyValuePair<Expression, Expression>> GetEnumerator() { return terms.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}