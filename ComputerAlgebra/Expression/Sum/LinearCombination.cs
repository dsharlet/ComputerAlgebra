using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        private void AddTerm(IEnumerable<Expression> B, Expression t)
        {
            foreach (Expression b in B)
            {
                if (Product.TermsOf(t).Count(i => i.Equals(b)) == 1)
                {
                    terms[b] += Product.New(Product.TermsOf(t).Except(b));
                    return;
                }
            }
            terms[1] += t;
        }

        /// <summary>
        /// Create a new linear combination expression equal to x.
        /// </summary>
        /// <param name="B">Basis terms for the linear combination.</param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static LinearCombination New(IEnumerable<Expression> B, Expression x)
        {
            B = B.AsBuffer();

            LinearCombination ret = new LinearCombination();
            foreach (Expression t in Sum.TermsOf(x.Expand()))
                ret.AddTerm(B, t);
            return ret;
        }

        /// <summary>
        /// Createa  new linear combination with the given list of terms.
        /// </summary>
        /// <param name="Terms"></param>
        /// <returns></returns>
        public static LinearCombination New(IEnumerable<KeyValuePair<Expression, Expression>> Terms) { return new LinearCombination(Terms); }

        // IEnumerable<KeyValuePair<Expression, Expression>>
        public IEnumerator<KeyValuePair<Expression, Expression>> GetEnumerator() { return terms.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}