﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Trivial implementation of Sum.
    /// </summary>
    class Add : Sum
    {
        protected IEnumerable<Expression> terms;
        public override IEnumerable<Expression> Terms { get { return terms; } }

        private Add(IEnumerable<Expression> Terms) { terms = Terms; }

        /// <summary>
        /// Create a new sum expression in canonical form.
        /// </summary>
        /// <param name="Terms">The list of terms in the sum expression.</param>
        /// <returns></returns>
        public static new Expression New(IEnumerable<Expression> Terms)
        {
            Debug.Assert(!Terms.Contains(null));

            // Canonicalize the terms.
            List<Expression> terms = FlattenTerms(Terms).ToList();
            terms.Sort();

            switch (terms.Count())
            {
                case 0: return 0;
                case 1: return terms.First();
                default: return new Add(terms);
            }
        }

        private static IEnumerable<Expression> FlattenTerms(IEnumerable<Expression> Terms)
        {
            foreach (Expression i in Terms)
            {
                if (i is Sum sum)
                    foreach (Expression j in FlattenTerms(sum.Terms))
                        yield return j;
                else if (!i.EqualsZero())
                    yield return i;
            }
        }
    }
}
