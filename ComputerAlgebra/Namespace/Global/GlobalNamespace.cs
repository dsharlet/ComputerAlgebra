using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Define the standard global namespace.
    /// </summary>
    class GlobalNamespace : TypeNamespace
    {
        // Globals with customized handling.
        private static Dictionary<string, IEnumerable<Expression>> lookups = new Dictionary<string, IEnumerable<Expression>>()
        {
            { "If", new[] { IfFunction.New() } },
            { "Sqrt", new[] { SqrtFunction.New() } },
            { "Exp", new[] { ExpFunction.New() } },
            { "Ln", new[] { LnFunction.New() } },
        };

        public GlobalNamespace() : base(typeof(GlobalNamespace)) { }

        public override IEnumerable<Expression> LookupName(string Name)
        {
            IEnumerable<Expression> lookup = null;
            if (lookups.TryGetValue(Name, out lookup))
                return lookup;
            return base.LookupName(Name);
        }

        // Some useful constants.
        public static readonly Expression Pi = Real.Pi;
        public static readonly Expression e = Real.e;

        public static readonly Expression False = Constant.New(false);
        public static readonly Expression True = Constant.New(true);

        public static Expression Abs(Constant x) { return Real.Abs(x); }
        public static Expression Sign(Constant x) { return Real.Sign(x); }

        public static Expression Min(Constant x, Constant y) { return Real.Min(x, y); }
        public static Expression Max(Constant x, Constant y) { return Real.Max(x, y); }

        public static Expression Sin(Constant x) { return Real.Sin(x); }
        public static Expression Cos(Constant x) { return Real.Cos(x); }
        public static Expression Tan(Constant x) { return Real.Tan(x); }
        public static Expression Sec(Constant x) { return Real.Sec(x); }
        public static Expression Csc(Constant x) { return Real.Csc(x); }
        public static Expression Cot(Constant x) { return Real.Cot(x); }

        public static Expression ArcSin(Constant x) { return Real.ArcSin(x); }
        public static Expression ArcCos(Constant x) { return Real.ArcCos(x); }
        public static Expression ArcTan(Constant x) { return Real.ArcTan(x); }
        public static Expression ArcSec(Constant x) { return Real.ArcSec(x); }
        public static Expression ArcCsc(Constant x) { return Real.ArcCsc(x); }
        public static Expression ArcCot(Constant x) { return Real.ArcCot(x); }

        public static Expression Sinh(Constant x) { return Real.Sinh(x); }
        public static Expression Cosh(Constant x) { return Real.Cosh(x); }
        public static Expression Tanh(Constant x) { return Real.Tanh(x); }
        public static Expression Sech(Constant x) { return Real.Sech(x); }
        public static Expression Csch(Constant x) { return Real.Csch(x); }
        public static Expression Coth(Constant x) { return Real.Coth(x); }

        public static Expression ArcSinh(Constant x) { return Real.ArcSinh(x); }
        public static Expression ArcCosh(Constant x) { return Real.ArcCosh(x); }
        public static Expression ArcTanh(Constant x) { return Real.ArcTanh(x); }
        public static Expression ArcSech(Constant x) { return Real.ArcSech(x); }
        public static Expression ArcCsch(Constant x) { return Real.ArcCsch(x); }
        public static Expression ArcCoth(Constant x) { return Real.ArcCoth(x); }

        public static Expression Log(Constant x, Constant b) { return Real.Log(x, b); }

        public static Expression Floor(Constant x) { return Real.Floor(x); }
        public static Expression Ceiling(Constant x) { return Real.Ceiling(x); }
        public static Expression Round(Constant x) { return Real.Round(x); }

        public static Expression Factorial(Constant x)
        {
            if (x.IsInteger())
                return new Real(Factorial((BigInteger)(Real)x));
            else
                throw new ArgumentException("Factorial cannot be called for non-integer value.");
        }

        public static Expression IsConstant(Constant x) { return Constant.New(true); }
        public static Expression IsInteger(Constant x) { return Constant.New(x.IsInteger()); }
        public static Expression IsNatural(Constant x) { return Constant.New(x.IsInteger() && (Real)x > 0); }

        public static Expression DependsOn(Expression f, Expression x) { return Constant.New(f.DependsOn(x)); }

        public static Expression Simplify(Expression x) { return x.Simplify(); }

        public static Expression Factor(Expression f) { return f.Factor(); }
        public static Expression Factor(Expression f, Expression x) { return f.Factor(x); }
        public static Expression Expand(Expression f) { return f.Expand(); }
        public static Expression Expand(Expression f, Expression x) { return f.Expand(x); }

        /// <summary>
        /// Solve a linear equation or system of linear equations f for expressions x.
        /// </summary>
        /// <param name="f">Equation or set of equations to solve.</param>
        /// <param name="x">Expressions to solve for.</param>
        /// <returns>Expression or set of expressions for x.</returns>
        public static Expression Solve(Expression f, Expression x)
        {
            IEnumerable<Expression> result = Set.MembersOf(f).Cast<Equal>().Solve(Set.MembersOf(x));
            return (f is Set || result.Count() != 1) ? Set.New(result) : result.Single();
        }
        /// <summary>
        /// Solve an equation or system of equations f for expressions x. Uses numerical solutions the equations are not linear equations of x.
        /// </summary>
        /// <param name="f">Equation or set of equations to solve.</param>
        /// <param name="x">Expressions to solve for.</param>
        /// <param name="e">Number of iterations to use for numerical solutions.</param>
        /// <returns>Expression or set of expressions for x.</returns>
        public static Expression NSolve(Expression f, Expression x, Expression e, Expression n)
        {
            IEnumerable<Expression> result = Set.MembersOf(f).Cast<Equal>().NSolve(Set.MembersOf(x).Cast<Arrow>(), (double)e, (int)n);
            return (f is Set || result.Count() != 1) ? Set.New(result) : result.Single();
        }
        public static Expression NSolve(Expression f, Expression x)
        {
            IEnumerable<Expression> result = Set.MembersOf(f).Cast<Equal>().NSolve(Set.MembersOf(x).Cast<Arrow>());
            return (f is Set || result.Count() != 1) ? Set.New(result) : result.Single();
        }

        /// <summary>
        /// Solve a differential equation or system of differential equations f for functions y[t], with initial conditions y^(n)[0] = y0.
        /// </summary>
        /// <param name="f">Equation or set of equations to solve.</param>
        /// <param name="y">Functions to solve for.</param>
        /// <param name="y0">Values for y^(n)[0].</param>
        /// <param name="t">Independent variable.</param>
        /// <returns>Expression or set of expressions for y.</returns>
        public static Expression DSolve(Expression f, Expression y, Expression y0, Expression t)
        {
            IEnumerable<Expression> result = Set.MembersOf(f).Cast<Equal>().DSolve(Set.MembersOf(y), Set.MembersOf(y0).Cast<Arrow>(), t);
            return (f is Set || result.Count() != 1) ? Set.New(result) : result.Single();
        }

        /// <summary>
        /// Differentiate f with respect to x.
        /// </summary>
        /// <param name="f">Expression to differentiate.</param>
        /// <param name="x">Differentiation variable.</param>
        /// <returns>Derivative of f with respect to x.</returns>
        public static Expression D(Expression f, [NoSubstitute] Expression x) { return f.Differentiate(x); }
        /// <summary>
        /// Integrate f with respect to x.
        /// </summary>
        /// <param name="f">Expression to integrate.</param>
        /// <param name="x">Integration variable.</param>
        /// <returns>Antiderivative of f with respect to x.</returns>
        public static Expression I(Expression f, [NoSubstitute] Expression x) { return f.Integrate(x); }
        /// <summary>
        /// Find the Laplace transform of f[t].
        /// </summary>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Expression L(Expression f, [NoSubstitute] Expression t, Expression s) { return f.LaplaceTransform(t, s); }
        /// <summary>
        /// Find the inverse Laplace transform of F[s].
        /// </summary>
        /// <param name="f"></param>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Expression IL(Expression f, [NoSubstitute] Expression s, Expression t) { return f.InverseLaplaceTransform(s, t); }


        private static BigInteger Factorial(BigInteger x)
        {
            BigInteger F = 1;
            while (x > 1)
                F = F * x--;
            return F;
        }
    };
}
