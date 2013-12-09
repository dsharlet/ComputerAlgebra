using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using ComputerAlgebra;
using ComputerAlgebra.Plotting;
using ComputerAlgebra.LinqCompiler;

namespace Demo
{

    class Program
    {
        // Some useful global variables.
        // Time t.
        static Expression t = "t";
        // Timestep.
        static double h = 0.01;
        // Number of points to evaluate the solution at.
        static int N = 100000;
        // Parameters of the Lotka-Volterra equations.
        static double a = 0.4;
        static double b = 0.01;
        static double c = 0.3;
        static double d = 0.005;

        // Initial conditions of the simulation.
        static double x0 = 100;
        static double y0 = 50;

        // Put the above parameters into a handy list for later evaluation.
        static List<Arrow> parameters = new List<Arrow>() 
        { 
            Arrow.New("a", a), 
            Arrow.New("b", b), 
            Arrow.New("c", c), 
            Arrow.New("d", d), 
        };

        // Define x(t) and y(t).
        static Expression xt = "x[t]";
        static Expression yt = "y[t]";

        // x(t0) = x(t - h and y(t0) = y(t - h).
        static Expression xt0 = xt.Evaluate(t, t - h);
        static Expression yt0 = yt.Evaluate(t, t - h);

        static void Main(string[] args)
        {
            // This program demonstrates how the ComputerAlgebra namespace can provide solutions to 
            // problems with the flexibility of runtime computed solutions to algebraic problems,
            // while maintaining the performance of natively coded solutions.

            // This example builds a simulation of the Lotka-Volterra equations, for more information
            // see http://en.wikipedia.org/wiki/Lotka%E2%80%93Volterra_equation
            
            // ------------------
            // Part 1. Building a solution to the Lotka-Volterra equations
                        

            // These are the variables and derivatives of our system:
            Expression dx_dt = "D[x[t], t]";
            Expression dy_dt = "D[y[t], t]";

            // Defining the equations. This uses the implicit conversion from string to Expression
            // to parse the expressions.
            List<Equal> equations = new List<Equal>()
            {
                Equal.New(dx_dt, "x[t]*(a - b*y[t])"),
                Equal.New(dy_dt, "-y[t]*(c - d*x[t])"),
            };
            Console.WriteLine("The equations to be solved are:");
            foreach (Expression i in equations)
                Console.WriteLine(i.ToPrettyString());
            Console.WriteLine();

            // We need to solve for dx_dt and dy_dt. In this example, this is trivial,
            // but this will handle any linear system of dx_dt, dy_dt.
            List<Arrow> derivatives = equations.Solve(dx_dt, dy_dt);
            Console.WriteLine("dx/dt and dy/dt are:");
            foreach (Expression i in derivatives)
                Console.WriteLine(i.ToPrettyString());
            Console.WriteLine();

            // Grab the actual values of dx/dt and dy/dt from the derivatives.
            dx_dt = dx_dt.Evaluate(derivatives);
            dy_dt = dy_dt.Evaluate(derivatives);

            // Now, let's use Euler's method to integrate these differentials. This defines 
            // x(t) and y(t) in terms of x(t0) and y(t0), where xt0 = x[t - h] and yt0 = y[t - h].
            List<Arrow> solutions = new List<Arrow>()
            {
                Arrow.New(xt, xt0 + h * dx_dt.Evaluate(t, t - h)),
                Arrow.New(yt, yt0 + h * dy_dt.Evaluate(t, t - h)),
            };
            Console.WriteLine("x(t) and y(t) in terms of x(t - h) and y(t - h) are:");
            foreach (Expression i in solutions)
                Console.WriteLine(i.ToPrettyString());
            Console.WriteLine();

            // Grab the actual solutions.
            Expression xSolution = xt.Evaluate(solutions);
            Expression ySolution = yt.Evaluate(solutions);

            // ------------------
            // Part 2. Evaluating the solution naively

            // Now, we can evaluate the solutions and plot the results.

            double naiveTime = EvaluateSolution(xSolution, ySolution);
            System.Console.WriteLine("Naive solution: {0} s", naiveTime);

            // ------------------
            // Part 3. Evaluating the solution via compiled expressions

            // The previous example works, but it's pretty slow. We can use the compiler functionality
            // of ComputerAlgebra.LinqCompiler to compile the solution to .Net code for much faster execution.

            double compiledTime = EvaluateCompiled(xSolution, ySolution);
            System.Console.WriteLine("Compiled solution: {0} s", compiledTime);

            // ------------------
            // Part 4. Comparison to native (hard-coded) code.

            // This is a *lot* faster than the naive evaluation, but what really matters is how it compares 
            // to a natively coded solution. Let's benchmark that now.

            double nativeTime = EvaluateNative();
            System.Console.WriteLine("Native solution: {0} s", nativeTime);

        }

        private static double EvaluateSolution(Expression xSolution, Expression ySolution)
        {
            // These lists store the values of x and y at each timestep for plotting.
            List<KeyValuePair<double, double>> xPoints = new List<KeyValuePair<double, double>>(N);
            List<KeyValuePair<double, double>> yPoints = new List<KeyValuePair<double, double>>(N);

            // Initial conditions for x and y.
            double x = x0;
            double y = y0;

            long begin = Timer.Counter;
            for (int n = 0; n < N; ++n)
            {
                // t at the current timestep.
                double tn = n * h;

                // Find x, y at the current t by substituting x, y as the previous timestep.
                double x_ = (double)xSolution.Evaluate(parameters.Append(Arrow.New(xt0, x), Arrow.New(yt0, y)));
                double y_ = (double)ySolution.Evaluate(parameters.Append(Arrow.New(xt0, x), Arrow.New(yt0, y)));

                // Store the current x, y.
                xPoints.Add(new KeyValuePair<double, double>(tn, x_));
                yPoints.Add(new KeyValuePair<double, double>(tn, y_));

                // Update the previous timestep.
                x = x_;
                y = y_;
            }
            double benchmark = Timer.Delta(begin);

            // Create a plot for the prey and predator functions.
            Plot plot = new Plot()
            {
                Title = "Naive Lotka-Volterra",
                x0 = 0,
                x1 = xPoints.Last().Key,
                xLabel = "Timestep",
                yLabel = "Population",
            };

            plot.Series.Add(new Scatter(xPoints.ToArray()) { Name = "Prey (x[t])" });
            plot.Series.Add(new Scatter(yPoints.ToArray()) { Name = "Predators (y[t])" });

            return benchmark;
        }

        private static double EvaluateCompiled(Expression xSolution, Expression ySolution)
        {
            var xSolutionCompiled = xSolution.Evaluate(parameters).Compile<Func<double, double, double>>(xt0, yt0);
            var ySolutionCompiled = ySolution.Evaluate(parameters).Compile<Func<double, double, double>>(xt0, yt0);

            var xPoints = new List<KeyValuePair<double, double>>(N);
            var yPoints = new List<KeyValuePair<double, double>>(N);

            // Initial conditions for x and y.
            double x = x0;
            double y = y0;

            long begin = Timer.Counter;
            for (int n = 0; n < N; ++n)
            {
                // t at the current timestep.
                double tn = n * h;

                // Find x, y at the current t by substituting x, y as the previous timestep.
                double x_ = (double)xSolutionCompiled(x, y);
                double y_ = (double)ySolutionCompiled(x, y);

                // Store the current x, y.
                xPoints.Add(new KeyValuePair<double, double>(tn, x_));
                yPoints.Add(new KeyValuePair<double, double>(tn, y_));

                // Update the previous timestep.
                x = x_;
                y = y_;
            }
            double benchmark = Timer.Delta(begin);

            // Create a plot for the prey and predator functions.
            Plot plot = new Plot()
            {
                Title = "Compiled Lotka-Volterra solution",
                x0 = 0,
                x1 = xPoints.Last().Key,
                xLabel = "Timestep",
                yLabel = "Population",
            };

            plot.Series.Add(new Scatter(xPoints.ToArray()) { Name = "Prey (x[t])" });
            plot.Series.Add(new Scatter(yPoints.ToArray()) { Name = "Predators (y[t])" });

            return benchmark;
        }

        private static double EvaluateNative()
        {
            var xPoints = new List<KeyValuePair<double, double>>(N);
            var yPoints = new List<KeyValuePair<double, double>>(N);

            // Initial conditions for x and y.
            double x = x0;
            double y = y0;

            long begin = Timer.Counter;
            for (int n = 0; n < N; ++n)
            {
                // t at the current timestep.
                double tn = n * h;

                // Find x, y at the current t by substituting x, y as the previous timestep.
                double x_ = h * a * x - h * b * x * y + x;
                double y_ = -h * c * y + h * d * x * y + y;

                // Store the current x, y.
                xPoints.Add(new KeyValuePair<double, double>(tn, x_));
                yPoints.Add(new KeyValuePair<double, double>(tn, y_));

                // Update the previous timestep.
                x = x_;
                y = y_;
            }
            double benchmark = Timer.Delta(begin);

            // Create a plot for the prey and predator functions.
            Plot plot = new Plot()
            {
                Title = "Native Lotka-Volterra solution",
                x0 = 0,
                x1 = xPoints.Last().Key,
                xLabel = "Timestep",
                yLabel = "Population",
            };

            plot.Series.Add(new Scatter(xPoints.ToArray()) { Name = "Prey (x[t])" });
            plot.Series.Add(new Scatter(yPoints.ToArray()) { Name = "Predators (y[t])" });

            return benchmark;
        }
    }

    public class Timer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public static long Counter
        {
            get
            {
                long t;
                QueryPerformanceCounter(out t);
                return t;
            }
        }

        public static double Frequency
        {
            get
            {
                long f;
                QueryPerformanceFrequency(out f);
                return (double)f;
            }
        }

        private long begin;
        public Timer() { begin = Counter; }

        public static double Delta(long t1) { return (double)(Counter - t1) / (double)Frequency; }

        public static implicit operator double(Timer T) { return (Counter - T.begin) / Frequency; }

        public override string ToString() { return ((Counter - begin) / Frequency).ToString("G3"); }
    }
}
