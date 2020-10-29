using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// RecursiveExpressionVisitor that caches the results of Visit. 
    /// The cache enables the CachedRecursiveVisitor to detect and avoid stack overflow situations.
    /// </summary>
    public class CachedRecursiveVisitor : RecursiveExpressionVisitor
    {
        private Dictionary<Expression, Expression> cache = new Dictionary<Expression, Expression>();

        public override Expression Visit(Expression E)
        {
            if (cache.TryGetValue(E, out Expression VE))
                return VE;

            return cache[E] = base.Visit(E);
        }
    }
}
