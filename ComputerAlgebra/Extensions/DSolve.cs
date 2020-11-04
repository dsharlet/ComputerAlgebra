using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Numerical methods for integration.
    /// </summary>
    public enum IntegrationMethod
    {
        Euler,
        BackwardEuler,
        Trapezoid,
        BackwardDifferenceFormula2,
    }

    /// <summary>
    /// Extensions for solving equations.
    /// </summary>
    public static class DSolveExtension
    {
        /// <summary>
        /// Solve a linear system of differential equations with initial conditions using the laplace transform.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="y"></param>
        /// <param name="y0"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Arrow> DSolve(this IEnumerable<Equal> f, IEnumerable<Expression> y, IEnumerable<Arrow> y0, Expression t)
        {
            // Find F(s) = L[f(t)] and substitute the initial conditions.
            List<Equal> F = f.Select(i => Equal.New(
                L(i.Left, t).Evaluate(y0),
                L(i.Right, t).Evaluate(y0))).ToList();

            // Solve F for Y(s) = L[y(t)].
            List<Arrow> Y = F.Solve(y.Select(i => L(i, t)));

            // Take L^-1[Y].
            Y = Y.Select(i => Arrow.New(IL(i.Left, t), IL(i.Right, t))).ToList();
            if (Y.DependsOn(s))
                throw new Exception("Could not find L^-1[Y(s)].");

            return Y;
        }

        /// <summary>
        /// Compute expressions for y[t] in terms of y[t - h], the resulting expressions will be implicit for implicit methods.
        /// </summary>
        /// <param name="dy_dt">Equations to solve.</param>
        /// <param name="y">Functions to solve for.</param>
        /// <param name="t">Independent variable.</param>
        /// <param name="h">Step size.</param>
        /// <param name="method">Integration method to use for differential equations.</param>
        /// <returns>Expressions for y[t].</returns>
        public static IEnumerable<Arrow> NDIntegrate(this IEnumerable<Arrow> dy_dt, Expression t, Expression h, IntegrationMethod method)
        {
            switch (method)
            {
                // y[t] = y[t - h] + h*f[t - h, y[t - h]]
                case IntegrationMethod.Euler:
                    return dy_dt.Select(i => Arrow.New(
                        DOf(i.Left),
                        DOf(i.Left).Substitute(t, t - h) + h * i.Right.Substitute(t, t - h)));

                // y[t] = y[t - h] + h*f[t, y[t]]
                case IntegrationMethod.BackwardEuler:
                    return dy_dt.Select(i => Arrow.New(
                        DOf(i.Left),
                        DOf(i.Left).Substitute(t, t - h) + h * i.Right));

                // y[t] = y[t - h] + (h/2)*(f[t - h, y[t - h]] + f[t, y[t]])
                case IntegrationMethod.Trapezoid:
                    return dy_dt.Select(i => Arrow.New(
                        DOf(i.Left),
                        DOf(i.Left).Substitute(t, t - h) + (h / 2) * (i.Right.Substitute(t, t - h) + i.Right)));

                // y[t] = (4/3)*y[t - h] - (1/3)y[t - 2h] + (2h/3)*f[t, y[t]]
                case IntegrationMethod.BackwardDifferenceFormula2:
                    return dy_dt.Select(i => Arrow.New(
                        DOf(i.Left),
                        4 * DOf(i.Left).Substitute(t, t - h) / 3 -
                        DOf(i.Left).Substitute(t, t - 2 * h) / 3 +
                        2 * h * i.Right / 3));

                default:
                    throw new NotImplementedException(method.ToString());
            }
        }

        /// <summary>
        /// Solve a linear system of differential equations for y[t] in terms of y[t - h].
        /// </summary>
        /// <param name="f">Equations to solve.</param>
        /// <param name="y">Functions to solve for.</param>
        /// <param name="t">Independent variable.</param>
        /// <param name="h">Step size.</param>
        /// <param name="method">Integration method to use for differential equations.</param>
        /// <returns>Expressions for y[t].</returns>
        public static List<Arrow> NDSolve(this IEnumerable<Equal> f, IEnumerable<Expression> y, Expression t, Expression h, IntegrationMethod method)
        {
            // Find y' in terms of y.
            List<Arrow> dy_dt = f.Solve(y.Select(i => D(i, t)));

            // If dy/dt appears on the right side of the system, the differential equation is not linear. Can't handle these.
            if (dy_dt.Any(i => i.Right.DependsOn(dy_dt.Select(j => j.Left))))
                throw new ArgumentException("Differential equation is singular or not linear.");

            return NDIntegrate(dy_dt, t, h, method)
                .Select(i => Equal.New(i.Left, i.Right))
                .Solve(y);
        }

        /// <summary>
        /// Partially solve a linear system of differential equations for y[t] in terms of y[t - h]. See SolveExtensions.PartialSolve for more information.
        /// </summary>
        /// <param name="f">Equations to solve.</param>
        /// <param name="y">Functions to solve for.</param>
        /// <param name="t">Independent variable.</param>
        /// <param name="h">Step size.</param>
        /// <param name="method">Integration method to use for differential equations.</param>
        /// <returns>Expressions for y[t].</returns>
        public static List<Arrow> NDPartialSolve(this IEnumerable<Equal> f, IEnumerable<Expression> y, Expression t, Expression h, IntegrationMethod method)
        {
            // Find y' in terms of y.
            List<Arrow> dy_dt = f.Solve(y.Select(i => D(i, t)));

            // If dy/dt appears on the right side of the system, the differential equation is not linear. Can't handle these.
            if (dy_dt.Any(i => i.Right.DependsOn(dy_dt.Select(j => j.Left))))
                throw new ArgumentException("Differential equation is singular or not linear.");

            return NDIntegrate(dy_dt, t, h, method)
                .Select(i => Equal.New(i.Left, i.Right))
                .PartialSolve(y);
        }

        // Get the expression that x is a derivative of.
        private static Expression DOf(Expression Dx)
        {
            Call d = (Call)Dx;
            if (d.Target.Name == "D")
                return d.Arguments.First();
            throw new InvalidCastException("Expression is not a derivative");
        }

        // Helpers.
        private static Expression s = Variable.New("_s");
        private static Expression L(Expression f, Expression t) { return f.LaplaceTransform(t, s); }
        private static Expression IL(Expression f, Expression t) { return f.InverseLaplaceTransform(s, t); }

        private static Expression D(Expression f, Expression t) { return Call.D(f, t); }
    }
}
