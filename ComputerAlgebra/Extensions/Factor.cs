using System;
using System.Collections.Generic;
using System.Linq;

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
            // If f is a product, just factor its terms.
            if (f is Product product)
                return Product.New(product.Terms.Select(i => i.Factor(x)));

            // If if is l^r, factor l and distribute r.
            if (f is Power power)
            {
                Expression l = power.Left.Factor(x);
                Expression r = power.Right;
                return Product.New(Product.TermsOf(l).Select(i => Power.New(i, r)));
            }

            // If f is a polynomial of x, use polynomial factoring methods.
            if (f is Polynomial p && (p.Variable.Equals(x) || (x is null)))
                return p.Factor();

            // Try interpreting f as a polynomial of x.
            if (!(x is null))
            {
                // If f is a polynomial of x, factor it.
                try
                {
                    return Polynomial.New(f, x).Factor();
                }
                catch (Exception) { }
            }

            // Just factor out common sub-expressions.
            if (f is Sum s)
            {
                // Make a list of each terms' products.
                List<List<Expression>> terms = s.Terms.Select(i => FactorsOf(i).ToList()).ToList();

                // All of the distinct factors.
                IEnumerable<Expression> factors = terms.SelectMany(i => i.Except(1, -1)).Distinct();
                // Choose the most common factor to factor.
                Expression factor = factors.ArgMax(i => terms.Count(j => j.Contains(i)));
                // Find the terms that contain the factor.
                List<List<Expression>> contains = terms.Where(i => i.Contains(factor)).ToList();
                // If more than one term contains the factor, pull it out and factor the resulting expressions (again).
                if (contains.Count() > 1)
                {
                    Expression factored = Sum.New(contains.Select(i => Product.New(i.Except(factor))));
                    Expression not_factored = Sum.New(terms.Except(contains).Select(i => Product.New(i)));
                    return Sum.New(Product.New(factor, factored), not_factored).Factor(null);
                }
            }
            return f;
        }

        public static Expression Factor(this Expression f) { return Factor(f, null); }
    }
}
