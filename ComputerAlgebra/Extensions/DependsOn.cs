using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Visitor for finding subexpressions in expressions. Returns null if any of the expressions is found.
    /// </summary>
    class SearchVisitor : RecursiveExpressionVisitor
    {
        protected IEnumerable<Expression> x;

        public SearchVisitor(IEnumerable<Expression> x) { this.x = x; }

        public override Expression Visit(Expression E)
        {
            if (x.Contains(E))
                return null;
            return base.Visit(E);
        }
    }

    public static class DependsOnExtension
    {
        /// <summary>
        /// Check if f is a function of any variable in x.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns>true if f is a function of any variable in x.</returns>
        public static bool DependsOn(this Expression f, IEnumerable<Expression> x)
        {
            return new SearchVisitor(x.Buffer()).Visit(f) is null;
        }
        public static bool DependsOn(this IEnumerable<Expression> f, IEnumerable<Expression> x)
        {
            SearchVisitor V = new SearchVisitor(x.Buffer());
            return f.Any(i => V.Visit(i) is null);
        }

        /// <summary>
        /// Check if f is a function of x.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns>true if f is a function of x.</returns>
        public static bool DependsOn(this Expression f, params Expression[] x) { return DependsOn(f, x.AsEnumerable()); }
        public static bool DependsOn(this IEnumerable<Expression> f, params Expression[] x) { return DependsOn(f, x.AsEnumerable()); }

        /// <summary>
        /// Check if f is a function of x.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns>true if f is a function of x.</returns>
        public static bool DependsOn(this Expression f, Expression x) { return DependsOn(f, Set.MembersOf(x)); }
        public static bool DependsOn(this IEnumerable<Expression> f, Expression x) { return DependsOn(f, Set.MembersOf(x)); }
    }
}
