using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using ComputerAlgebra;
using ComputerAlgebra.Plotting;
using ComputerAlgebra.LinqCompiler;

using LinqExprs = System.Linq.Expressions;
using LinqExpr = System.Linq.Expressions.Expression;

namespace Demo
{
    // This program demonstrates how the ComputerAlgebra namespace can provide solutions to 
    // problems with the flexibility of runtime computed solutions to algebraic problems,
    // while maintaining the performance of natively coded solutions.

    // This example builds a simulation of the competitive Lotka-Volterra equations, for more 
    // information, see http://en.wikipedia.org/wiki/Competitive_Lotka%E2%80%93Volterra_equations

    /// <summary>
    /// This class describes an ecosystem for a set of species.
    /// </summary>
    class PopulationSystem
    {
        private double[] _x;
        /// <summary>
        /// The current population x of the system.
        /// </summary>
        public double[] x { get { return _x; } }

        /// <summary>
        /// The number of species in this system.
        /// </summary>
        public int N { get { return _x.Length; } }

        private double[,] _A;
        /// <summary>
        /// The community matrix A of the system.
        /// </summary>
        public double[,] A { get { return _A; } }

        private double[] _r;
        /// <summary>
        /// The intrinsic birth/death rates r of the system.
        /// </summary>
        public double[] r { get { return _r; } }


        /// <summary>
        /// Construct a new population system with N species.
        /// </summary>
        /// <param name="N"></param>
        public PopulationSystem(int N)
        {
            _A = new double[N, N];
            _r = new double[N];
            _x = new double[N];
        }

        /// <summary>
        /// Construct a new populatuion system with the given initial population.
        /// </summary>
        /// <param name="x0"></param>
        public PopulationSystem(double[] x0) : this(x0.Length)
        {
            for (int i = 0; i < x0.Length; ++i)
                _x[i] = x0[i];
        }
    };

    class Program
    {        
        static void Main(string[] args)
        {
            // The number of timesteps simulated.
            const int N = 100000;
            // Time between timesteps.
            const double dt = 0.01;

            // First, we define an example population system description:
            PopulationSystem S = new PopulationSystem(4);

            S.x[0] = 0.2;
            S.x[1] = 0.4586;
            S.x[2] = 0.1307;
            S.x[3] = 0.3557;

            S.r[0] = 1;
            S.r[1] = 0.72;
            S.r[2] = 1.53;
            S.r[3] = 1.27;

            S.A[0, 0] = 1;      S.A[0, 1] = 1.09;   S.A[0, 2] = 1.52;   S.A[0, 3] = 0;
            S.A[1, 0] = 0;      S.A[1, 1] = 1;      S.A[1, 2] = 0.44;   S.A[1, 3] = 1.36;
            S.A[2, 0] = 2.33;   S.A[2, 1] = 0;      S.A[2, 2] = 1;      S.A[2, 3] = 0.47;
            S.A[3, 0] = 1.21;   S.A[3, 1] = 0.51;   S.A[3, 2] = 0.35;   S.A[3, 3] = 1;

            Expression t = "t";

            // Array of doubles representing the populations over time.
            double[,] data = new double[N, S.N];

            // Initialize the system to have the population specified in the system.
            for (int i = 0; i < S.N; ++i)
                data[0, i] = S.x[i];

            // Run N timesteps.
            long start = Timer.Counter;
            //for (int n = 1; n < N; ++n)
            //{
            //    for (int i = 0; i < S.N; ++i)
            //    {
            //        double dx_dt = 0.0;
            //        for (int j = 0; j < S.N; ++j)
            //            dx_dt += S.A[i, j] * x[j];
            //        dx_dt *= S.r[i] * x[i] * (1 - dx_dt);

            //        data[n, i] = data[n - 1, i] + dt * dx_dt;
            //    }
            //}

            // The population variable for species 'i' is x_i[t], i.e. a 
            // variable dependent on t.

            CodeGen integrate = new CodeGen();

            // Define a parameter for the current population x, and define mappings to the expressions defined above.
            LinqExpr _x = integrate.Decl<double[]>(Scope.Parameter, "x");
            List<Expression> x = new List<Expression>();
            for (int i = 0; i < S.N; ++i)
            {
                // Define a variable xi.
                Expression xi = "x" + i.ToString();
                x.Add(xi);
                // Map xi to the parameter x[i].
                integrate.Map(Scope.Local, xi, LinqExpr.ArrayAccess(_x, LinqExpr.Constant(i)));
            }

            for (int i = 0; i < S.N; ++i)
            {
                // This list is the elements of the sum representing the i'th 
                // row of f, i.e. r_i + (A*x)_i.
                List<Expression> terms = new List<Expression>();
                for (int j = 0; j < S.N; ++j)
                    terms.Add(S.A[i, j] * x[j]);

                // Define dx_i/dt = x_i * f_i(x), as per the Lotka-Volterra equations.
                Expression dx_dt = x[i] * S.r[i] * (1 - Sum.New(terms));

                // Euler's method for x(t) is: x(t) = x(t - h) + h * x'(t - h).
                Expression integral = x[i] + dt * dx_dt;
       
            }
                       

            Console.WriteLine("Native code time: {0}", Timer.Delta(start));

            // Plot the resulting data.
            Plot plot = new Plot()
            {
                x0 = 0.0,
                x1 = dt * N,
                Title = "Native Solution",
                xLabel = "Time",
                yLabel = "Population",
            };
            for (int i = 0; i < S.N; ++i)
            {
                KeyValuePair<double, double>[] points = new KeyValuePair<double,double>[N];
                for (int j = 0; j < N; ++j)
                    points[j] = new KeyValuePair<double,double>(j * dt, data[j, i]);
                plot.Series.Add(new Scatter(points) { Name = i.ToString() });
            }
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
