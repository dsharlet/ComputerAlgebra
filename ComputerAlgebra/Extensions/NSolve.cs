using ComputerAlgebra.LinqCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using LinqExpr = System.Linq.Expressions.Expression;
using ParamExpr = System.Linq.Expressions.ParameterExpression;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception for iterative algorithm convergence issues.
    /// </summary>
    public class FailedToConvergeException : ArithmeticException
    {
        public FailedToConvergeException(string Message) : base(Message) { }
        public FailedToConvergeException(string Message, Exception Inner) : base(Message, Inner) { }
    }

    /// <summary>
    /// Extensions for solving equations.
    /// </summary>
    public static class NSolveExtension
    {
        /// <summary>
        /// Compute the gradient of f(x).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Dictionary<Expression, Expression> Gradient(this Expression f, IEnumerable<Expression> x)
        {
            return x.ToDictionary(i => i, i => f.Differentiate(i));
        }

        /// <summary>
        /// Compute the Jacobian of F(x).
        /// </summary>
        /// <param name="F"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static List<Dictionary<Expression, Expression>> Jacobian(this IEnumerable<Expression> F, IEnumerable<Expression> x)
        {
            return F.Select(i => i.Gradient(x)).ToList();
        }

        // Use neton's method to solve F(x) = 0, with initial guess x0.
        private static void NewtonsMethod(int M, int N, Func<double[,], double[], double, double> J, double s, double[] x, double Epsilon, int MaxIterations)
        {
            // Numerically approximate the solution of F = 0 with Newton's method, 
            // i.e. solve JF(x0)*(x - x0) = -F(x0) for x.

            double[,] JxF = new double[M, N + 1];
            double[] dx = new double[N];

            double epsilon = Epsilon * Epsilon * N;
            try
            {
                for (int n = 0; n < MaxIterations; ++n)
                {
                    // Evaluate JxF and F.
                    if (J(JxF, x, s) < epsilon)
                        return;

                    // Solve for dx.
                    // For each variable in the system...
                    for (int j = 0; j + 1 < N; ++j)
                    {
                        int pi = j;
                        double max = Math.Abs(JxF[j, j]);

                        // Find a pivot row for this variable.
                        for (int i = j + 1; i < M; ++i)
                        {
                            // if(|JxF[i][j]| > max) { pi = i, max = |JxF[i][j]| }
                            double maxj = Math.Abs(JxF[i, j]);
                            if (maxj > max)
                            {
                                pi = i;
                                max = maxj;
                            }
                        }

                        // Swap pivot row with the current row.
                        if (pi != j)
                            for (int ij = j; ij <= N; ++ij)
                                Swap(ref JxF[j, ij], ref JxF[pi, ij]);

                        // Eliminate the rows after the pivot.
                        double p = JxF[j, j];
                        for (int i = j + 1; i < M; ++i)
                        {
                            double e = JxF[i, j] / p;
                            if (e != 0.0)
                                for (int ij = j + 1; ij <= N; ++ij)
                                    JxF[i, ij] -= JxF[j, ij] * e;
                        }
                    }

                    // JxF is now upper triangular, solve it.
                    for (int j = N - 1; j >= 0; --j)
                    {
                        double r = JxF[j, N];
                        for (int ij = j + 1; ij < N; ++ij)
                            r += JxF[j, ij] * dx[ij];

                        if (JxF[j, j] == 0.0)
                            throw new ArithmeticException("Singular system of equations.");

                        double dxj = -r / JxF[j, j];
                        dx[j] = dxj;
                        x[j] += dxj;
                    }
                }
            }
            catch (ArithmeticException Ex)
            {
                throw new FailedToConvergeException("Newton's method failed to converge.", Ex);
            }

            // Failed to converge.
            throw new FailedToConvergeException("NSolve failed to converge.");
        }

        private static void Swap(ref double a, ref double b) { double t = a; a = b; b = t; }

        // Use homotopy method with newton's method to find a solution for F(x) = 0.
        private static List<Arrow> NSolve(List<Expression> F, List<Arrow> x0, double Epsilon, int MaxIterations)
        {
            int M = F.Count;
            int N = x0.Count;

            // Compute JxF, the Jacobian of F.
            List<Dictionary<Expression, Expression>> JxF = Jacobian(F, x0.Select(i => i.Left)).ToList();

            // Define a function to evaluate JxH(x), where H = F(x) - s*F(x0). 
            CodeGen code = new CodeGen();

            ParamExpr _JxH = code.Decl<double[,]>(Scope.Parameter, "JxH");
            ParamExpr _x0 = code.Decl<double[]>(Scope.Parameter, "x0");
            ParamExpr _s = code.Decl<double>(Scope.Parameter, "s");

            // Load x_j from the input array and add them to the map.
            for (int j = 0; j < N; ++j)
                code.DeclInit(x0[j].Left, LinqExpr.ArrayAccess(_x0, LinqExpr.Constant(j)));

            LinqExpr error = code.Decl<double>("error");

            // Compile the expressions to assign JxH
            for (int i = 0; i < M; ++i)
            {
                LinqExpr _i = LinqExpr.Constant(i);
                for (int j = 0; j < N; ++j)
                    code.Add(LinqExpr.Assign(
                        LinqExpr.ArrayAccess(_JxH, _i, LinqExpr.Constant(j)),
                        code.Compile(JxF[i][x0[j].Left])));
                // e = F(x) - s*F(x0)
                LinqExpr e = code.DeclInit<double>("e", LinqExpr.Subtract(code.Compile(F[i]), LinqExpr.Multiply(LinqExpr.Constant((double)F[i].Evaluate(x0)), _s)));
                code.Add(LinqExpr.Assign(LinqExpr.ArrayAccess(_JxH, _i, LinqExpr.Constant(N)), e));
                // error += e * e
                code.Add(LinqExpr.AddAssign(error, LinqExpr.Multiply(e, e)));
            }

            // return error
            code.Return(error);

            Func<double[,], double[], double, double> JxH = code.Build<Func<double[,], double[], double, double>>().Compile();

            double[] x = new double[N];

            // Remember where we last succeeded/failed.
            double s0 = 0.0;
            double s1 = 1.0;
            do
            {
                try
                {
                    // H(F, s) = F - s*F0
                    NewtonsMethod(M, N, JxH, s0, x, Epsilon, MaxIterations);

                    // Success at this s!
                    s1 = s0;
                    for (int i = 0; i < N; ++i)
                        x0[i] = Arrow.New(x0[i].Left, x[i]);

                    // Go near the goal.
                    s0 = Lerp(s0, 0.0, 0.9);
                }
                catch (FailedToConvergeException)
                {
                    // Go near the last success.
                    s0 = Lerp(s0, s1, 0.9);

                    for (int i = 0; i < N; ++i)
                        x[i] = (double)x0[i].Right;
                }
            } while (s0 > 0.0 && s1 >= s0 + 1e-6);

            // Make sure the last solution is at F itself.
            if (s0 != 0.0)
            {
                NewtonsMethod(M, N, JxH, 0.0, x, Epsilon, MaxIterations);
                for (int i = 0; i < N; ++i)
                    x0[i] = Arrow.New(x0[i].Left, x[i]);
            }

            return x0;
        }

        private static double Lerp(double a, double b, double t) { return a + (b - a) * t; }

        /// <summary>
        /// Numerically solve a system of equations implicitly equal to 0.
        /// </summary>
        /// <param name="f">System of equations to solve for 0.</param>
        /// <param name="x">List of variables to solve for, with an initial guess.</param>
        /// <param name="Epsilon">Threshold for convergence.</param>
        /// <returns></returns>
        public static List<Arrow> NSolve(this IEnumerable<Equal> f, IEnumerable<Arrow> x, double Epsilon, int MaxIterations)
        {
            return NSolve(f.Select(i => EqualToZero(i)).AsList(), x.AsList(), Epsilon, MaxIterations);
        }

        /// <summary>
        /// Numerically solve a system of equations implicitly equal to 0.
        /// </summary>
        /// <param name="f">System of equations to solve for 0.</param>
        /// <param name="x">List of variables to solve for, with an initial guess.</param>
        /// <returns></returns>
        public static List<Arrow> NSolve(this IEnumerable<Equal> f, IEnumerable<Arrow> x) { return NSolve(f, x, 1e-6, 64); }

        private static Expression EqualToZero(Expression i)
        {
            if (i is Equal equal)
                return equal.Left - equal.Right;
            else
                return i;
        }
    }
}
