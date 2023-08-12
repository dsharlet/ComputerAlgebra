﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Trivial implementation of Product.
    /// </summary>
    class Multiply : Product
    {
        protected IEnumerable<Expression> terms;
        public override IEnumerable<Expression> Terms { get { return terms; } }

        private Multiply(IEnumerable<Expression> Terms) { terms = Terms; }

        /// <summary>
        /// Create a new product expression in canonical form.
        /// </summary>
        /// <param name="Terms">The list of terms in the product expression.</param>
        /// <returns></returns>
        public static new Expression New(IEnumerable<Expression> Terms)
        {
            // Canonicalize the terms.
            List<Expression> terms = FlattenTerms(Terms).ToList();
            terms.Sort();

            switch (terms.Count())
            {
                case 0: return 1;
                case 1: return terms.First();
                default: return new Multiply(terms);
            }
        }

        private static IEnumerable<Expression> FlattenTerms(IEnumerable<Expression> Terms)
        {
            foreach (Expression i in Terms)
            {
                if (i is Product product)
                    foreach (Expression j in FlattenTerms(product.Terms))
                        yield return j;
                else if (!i.EqualsOne())
                    yield return i;
            }
        }
    }
}
