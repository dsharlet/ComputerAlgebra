using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Expression visitor for performing namespace lookups on unresolved variables/calls.
    /// </summary>
    class ResolveVisitor : RecursiveExpressionVisitor
    {
        protected Namespace ns;

        public ResolveVisitor(Namespace Namespace) { ns = Namespace; }

        protected override Expression VisitCall(Call F)
        {
            F = (Call)base.VisitCall(F);

            if (F.Target is UnknownFunction)
            {
                IEnumerable<Function> lookup = ns.LookupFunction(F.Target.Name, F.Arguments);
                switch (lookup.Count())
                {
                    case 0: return F;
                    case 1: return Call.New(lookup.Single(), F.Arguments);
                    default: throw new UnresolvedName(F.Target.Name);
                }
            }

            return F;
        }

        protected override Expression VisitVariable(Variable V)
        {
            IEnumerable<Expression> lookup = ns.LookupName(V.Name);
            switch (lookup.Count())
            {
                case 0: return V;
                case 1: return lookup.Single();
                default: throw new UnresolvedName(V.Name);
            }
        }
    }

    public static class ResolveExtension
    {
        /// <summary>
        /// Substitute variables x0 into f.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x0"></param>
        /// <returns></returns>
        public static Expression Resolve(this Expression f, Namespace Namespace)
        {
            return new ResolveVisitor(Namespace).Visit(f);
        }

        /// <summary>
        /// Substitute variables x0 into f.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x0"></param>
        /// <returns></returns>
        public static IEnumerable<Expression> Resolve(this IEnumerable<Expression> f, Namespace Namespace)
        {
            ResolveVisitor V = new ResolveVisitor(Namespace);
            return f.Select(i => V.Visit(i));
        }
    }
}
