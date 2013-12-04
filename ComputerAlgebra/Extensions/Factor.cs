using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    public static class FactorExtension
    {
        // Enumerates x, splitting negative constants into a positive constant and -1.
        private static IEnumerable<Expression> FactorsOf(Expression x)
        {
            foreach (Expression i in Product.TermsOf(x))
            {
                if (i is Constant && (Real)i < 0)
                {
                    yield return -1;
                    yield return Real.Abs((Real)i);
                }
                else if (i is Power)
                {
                    yield return i;
                    yield return ((Power)i).Left;
                }
                else
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Distribute products across sums.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Expression Factor(this Expression f, Expression x) 
        {
            if (f is Product)
                return Product.New(((Product)f).Terms.Select(i => i.Factor(x)));

            if (f is Power)
            {
                Expression l = ((Power)f).Left.Factor(x);
                Expression r = ((Power)f).Right;
                return Product.New(Product.TermsOf(l).Select(i => Power.New(i, r)));
            }

            if (!ReferenceEquals(x, null))
            {
                // If f is a polynomial of x, factor it.
                try
                {
                    return Polynomial.New(f, x).Factor();
                }
                catch (Exception) { }
            }

            if (f is Sum)
            {
                Sum s = (Sum)f;

                IEnumerable<Expression> terms = s.Terms.Select(i => i.Factor()).Buffer();
                
                // All of the distinct factors.
                IEnumerable<Expression> factors = terms.SelectMany(i => FactorsOf(i)).Distinct();
                // Choose the most common factor to use.
                Expression factor = factors.ArgMax(i => terms.Count(j => FactorsOf(j).Contains(i)));
                // Find the terms that contain the factor.
                IEnumerable<Expression> contains = terms.Where(i => FactorsOf(i).Contains(factor)).Buffer();
                // If more than one term contains the factor, pull it out and factor the resulting expression (again).
                if (contains.Count() > 1)
                    return Sum.New(
                        Product.New(factor, Sum.New(contains.Select(i => Binary.Divide(i, factor))).Evaluate()),
                        Sum.New(terms.Except(contains, Expression.RefComparer))).Factor(null);
            }

            return f;
        }

        public static Expression Factor(this Expression f) { return Factor(f, null); }
    }
}
