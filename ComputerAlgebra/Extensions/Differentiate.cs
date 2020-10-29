using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Implements differentiation.
    /// </summary>
    class DifferentiateTransform : VisitorTransform
    {
        // Rule database.
        private static TransformSet rules = new TransformSet()
        {
            new SubstituteTransform("D[Sin[u], x]", "Cos[u]*D[u, x]"),
            new SubstituteTransform("D[Cos[u], x]", "-Sin[u]*D[u, x]"),
            new SubstituteTransform("D[Tan[u], x]", "Sec[u]^2*D[u, x]"),
            new SubstituteTransform("D[Sec[u], x]", "Sec[u]*Tan[u]*D[u, x]"),
            new SubstituteTransform("D[Csc[u], x]", "-Csc[u]*Cot[u]*D[u, x]"),
            new SubstituteTransform("D[Cot[u], x]", "-Csc[u]^2*D[u, x]"),

            new SubstituteTransform("D[ArcSin[u], x]", "D[u, x]/Sqrt[1 - u^2]"),
            new SubstituteTransform("D[ArcCos[u], x]", "-D[u, x]/Sqrt[1 - u^2]"),
            new SubstituteTransform("D[ArcTan[u], x]", "D[u, x]/(u^2 + 1)"),
            new SubstituteTransform("D[ArcSec[u], x]", "D[u, x]/(Abs[u]*Sqrt[u^2 - 1])"),
            new SubstituteTransform("D[ArcCsc[u], x]", "-D[u, x]/(Abs[u]*Sqrt[u^2 - 1])"),
            new SubstituteTransform("D[ArcCot[u], x]", "-D[u, x]/(u^2 + 1)"),

            new SubstituteTransform("D[Sinh[u], x]", "Cosh[u]*D[u, x]"),
            new SubstituteTransform("D[Cosh[u], x]", "Sinh[u]*D[u, x]"),
            new SubstituteTransform("D[Tanh[u], x]", "Sech[u]^2*D[u, x]"),
            new SubstituteTransform("D[Sech[u], x]", "-Sech[u]*Tanh[u]*D[u, x]"),
            new SubstituteTransform("D[Csch[u], x]", "-Csch[u]*Coth[u]*D[u, x]"),
            new SubstituteTransform("D[Coth[u], x]", "-Csch[u]^2*D[u, x]"),

            new SubstituteTransform("D[ArcSinh[u], x]", "D[u, x]/Sqrt[u^2 + 1]"),
            new SubstituteTransform("D[ArcCosh[u], x]", "D[u, x]/Sqrt[u^2 - 1]"),
            new SubstituteTransform("D[ArcTanh[u], x]", "D[u, x]/(1 - u^2)"),
            new SubstituteTransform("D[ArcSech[u], x]", "-D[u, x]/(u*Sqrt[1 - u^2])"),
            new SubstituteTransform("D[ArcCsch[u], x]", "-D[u, x]/(Abs[u]*Sqrt[u^2 + 1])"),
            new SubstituteTransform("D[ArcCoth[u], x]", "-D[u, x]/(1 - u^2)"),

            new SubstituteTransform("D[Abs[u], x]", "Sign[u]*D[u, x]"),
            new SubstituteTransform("D[Sign[u], x]", "0"),
            new SubstituteTransform("D[Exp[u], x]", "Exp[u]*D[u, x]"),
            //new SubstituteTransform("D[Sqrt[u], x]", "D[u, x]/(2*Sqrt[u])"),
            new SubstituteTransform("D[Ln[u], x]", "D[u, x]/u"),
            new SubstituteTransform("D[I[u, x], x]", "u"),
            new SubstituteTransform("D[If[c, t, f], x]", "If[c, D[t, x], D[f, x]]"),
            new SubstituteTransform("D[Max[a, b], x]", "If[a > b, D[a, x], D[b, x]]"),
            new SubstituteTransform("D[Min[a, b], x]", "If[a < b, D[a, x], D[b, x]]")
        };

        // Differentation variable.
        protected Expression x;

        private DifferentiateTransform(Expression x) { this.x = x; }

        public static Expression Transform(Expression f, Expression x) { return new DifferentiateTransform(x).Visit(f); }

        protected override Expression VisitSum(Sum A) 
        {
            List<Expression> terms = new List<Expression>();
            foreach (Expression i in A.Terms)
                terms.Add(Visit(i));
            return EvaluateVisitor.EvaluateSum(terms); 
        }

        protected override Expression VisitProduct(Product M)
        {
            List<Expression> independent = new List<Expression>();
            List<Expression> dependent = new List<Expression>();
            foreach (Expression i in M.Terms)
            {
                if (i.DependsOn(x))
                    dependent.Add(i);
                else
                    independent.Add(i);
            }
            if (dependent.Count == 0)
                return 0;

            List<Expression> products = new List<Expression>(dependent.Count);
            foreach (Expression i in dependent)
            {
                List<Expression> terms = new List<Expression>(dependent.Count);
                foreach (Expression j in dependent)
                {
                    if (ReferenceEquals(i, j))
                        terms.Add(Visit(i));
                    else
                        terms.Add(j);
                }
                products.Add(Product.New(terms));
            }
            return Product.New(Product.New(independent), Sum.New(products)).Evaluate();
        }

        protected override Expression VisitPower(Power P)
        {
            Expression f = P.Left;
            Expression g = P.Right;
            if (g.DependsOn(x))
            {
                // f(x)^g(x)
                return Product.New(P,
                    Sum.New(
                        Product.New(Visit(f), Binary.Divide(g, f)),
                        Product.New(Visit(g), Call.Ln(f)))).Evaluate();
            }
            else
            {
                // f(x)^g
                return Product.New(
                    g,
                    Power.New(f, Binary.Subtract(g, 1)),
                    Visit(f)).Evaluate();
            }
        }

        protected override Expression VisitBinary(Binary B)
        {
            // Logic operators are mostly constant.
            if (B.IsLogicOp)
                return 0;
            return base.VisitBinary(B);
        }

        protected override Expression VisitUnary(Unary U)
        {
            // Logic operators are mostly constant.
            if (U.IsLogicOp)
                return 0;
            return base.VisitUnary(U);
        }

        protected override Expression VisitUnknown(Expression E)
        {
            if (E.Equals(x))
                return 1;
            else if (E.DependsOn(x))
                return rules.Transform(Call.D(E, x));
            else
                return 0;
        }
    }

    public static class DifferentiateExtension
    {
        /// <summary>
        /// Differentiate expression with respect to x.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Expression Differentiate(this Expression f, Expression x) { return DifferentiateTransform.Transform(f, x); }
    }
}
