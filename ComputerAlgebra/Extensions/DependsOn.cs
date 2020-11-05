using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Visitor for finding subexpressions in expressions. Returns null if any of the expressions is found.
    /// </summary>
    class DependsOnVisitor : ExpressionVisitor<bool>
    {
        protected IEnumerable<Expression> x;

        public DependsOnVisitor(IEnumerable<Expression> x) { this.x = x; }

        public override bool Visit(Expression E)
        {
            if (x.Contains(E))
                return true;
            return base.Visit(E);
        }

        protected override bool VisitBinary(Binary B)
        {
            return Visit(B.Left) || Visit(B.Right);
        }

        protected override bool VisitCall(Call F)
        {
            return Visit(F.Target) || F.Arguments.Any(i => Visit(i));
        }

        protected override bool VisitMatrix(Matrix A)
        {
            return A.Any(i => Visit(i));
        }

        protected override bool VisitPower(Power P)
        {
            return VisitBinary(P);
        }

        protected override bool VisitProduct(Product M)
        {
            return M.Terms.Any(i => Visit(i));
        }

        protected override bool VisitSet(Set S)
        {
            return S.Members.Any(i => Visit(i));
        }

        protected override bool VisitSum(Sum A)
        {
            return A.Terms.Any(i => Visit(i));
        }

        protected override bool VisitUnary(Unary U)
        {
            return Visit(U.Operand);
        }

        protected override bool VisitUnknown(Expression E)
        {
            return false;
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
            return new DependsOnVisitor(x.Buffer()).Visit(f);
        }
        public static bool DependsOn(this IEnumerable<Expression> f, IEnumerable<Expression> x)
        {
            DependsOnVisitor V = new DependsOnVisitor(x.Buffer());
            return f.Any(i => V.Visit(i));
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
