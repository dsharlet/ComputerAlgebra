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
        // Run the simulation of the population system S for N timesteps of length dt.
        // The population at each timestep is stored in the rows of Data.
        static void SimulateNative(int N, double dt, double[,] Data, PopulationSystem S)
        {
            // Loop over the sample range requested.
            for (int n = 1; n < N; ++n)
            {
                // Loop over the species and compute the population change.
                for (int i = 0; i < S.N; ++i)
                {
                    double dx_dt = 1.0;
                    for (int j = 0; j < S.N; ++j)
                        dx_dt -= S.A[i, j] * Data[n - 1, j];
                    dx_dt *= S.r[i] * Data[n - 1, i];

                    Data[n, i] = Data[n - 1, i] + dt * dx_dt;
                }
            }
        }
        
        // Define a function for running the simulation of a population system S with timestep
        // dt. The number of timesteps and the data buffer are parameters of the defined function.
        static Func<int, double[,], int> DefineSimulate(double dt, PopulationSystem S)
        {
            // The population variable for species 'i' is x_i[t], i.e. a 
            // variable dependent on t.

            CodeGen code = new CodeGen();

            // Define a parameter for the current population x, and define mappings to the 
            // expressions defined above.
            LinqExpr N = code.Decl<int>(Scope.Parameter, "N");
            LinqExpr Data = code.Decl<double[,]>(Scope.Parameter, "Data");

            // Loop over the sample range requested. Note that this loop is a 'runtime' loop,
            // while the rest of the loops nested in the body of this loop are 'compile time' loops.
            LinqExpr n = code.DeclInit<int>("n", 1);
            code.For(
                () => { }, 
                LinqExpr.LessThan(n, N), 
                () => code.Add(LinqExpr.PostIncrementAssign(n)), 
                () =>
            {
                // Define expressions representing the population of each species.
                List<Expression> x = new List<Expression>();
                for (int i = 0; i < S.N; ++i)
                {
                    // Define a variable xi.
                    Expression xi = "x" + i.ToString();
                    x.Add(xi);
                    // Map xi to Data[n, i].
                    code.Map(Scope.Local, xi, LinqExpr.ArrayAccess(Data, LinqExpr.Subtract(n, LinqExpr.Constant(1)), LinqExpr.Constant(i)));
                }

                for (int i = 0; i < S.N; ++i)
                {
                    // This list is the elements of the sum representing the i'th 
                    // row of f, i.e. r_i + (A*x)_i.
                    Expression dx_dt = 1;
                    List<Expression> terms = new List<Expression>();
                    for (int j = 0; j < S.N; ++j)
                        dx_dt -= S.A[i, j] * x[j];

                    // Define dx_i/dt = x_i * f_i(x), as per the Lotka-Volterra equations.
                    dx_dt *= x[i] * S.r[i];

                    // Euler's method for x(t) is: x(t) = x(t - h) + h * x'(t - h).
                    Expression integral = x[i] + dt * dx_dt;

                    // Data[n, i] = Data[n - 1, i] + dt * dx_dt;
                    code.Add(LinqExpr.Assign(
                        LinqExpr.ArrayAccess(Data, n, LinqExpr.Constant(i)), 
                        code.Compile(integral)));
                }
            });

            code.Return(N);

            // Compile the generated code.
            LinqExprs.Expression<Func<int, double[,], int>> expr = code.Build<Func<int, double[,], int>>();
            return expr.Compile();
        }

        // The same as SimulateNative, but hardcoded for the particular system defined below.
        // This solution is obviously very inflexible!
        static void SimulateNativeHardCoded(int N, double[,] Data)
        {
            // Loop over the sample range requested.
            for (int n = 1; n < N; ++n)
            {
                double x0 = Data[n - 1, 0];
                double x1 = Data[n - 1, 1];
                double x2 = Data[n - 1, 2];
                double x3 = Data[n - 1, 3];

                double dx0_dt = 1.0
                    - 1 * x0
                    - 1.09 * x1
                    - 1.52 * x2
                    - 0 * x3;
                double dx1_dt = 1.0
                    - 0 * x0
                    - 1 * x1
                    - 0.44 * x2
                    - 1.36 * x3;
                double dx2_dt = 1.0
                    - 2.33 * x0
                    - 0 * x1
                    - 1 * x2
                    - 0.47 * x3;
                double dx3_dt = 1.0
                    - 1.21 * x0
                    - 0.51 * x1
                    - 0.35 * x2
                    - 1 * x3;

                dx0_dt *= 0.001 * 1 * x0;
                dx1_dt *= 0.001 * 0.72 * x1;
                dx2_dt *= 0.001 * 1.53 * x2;
                dx3_dt *= 0.001 * 1.27 * x3;

                Data[n, 0] = x0 + dx0_dt;
                Data[n, 1] = x1 + dx1_dt;
                Data[n, 2] = x2 + dx2_dt;
                Data[n, 3] = x3 + dx3_dt;
            }
        }

        static double RunSimulation(Action<int, double, double[,]> Simulate, int N, double dt, PopulationSystem S, string Title)
        {
            // Array of doubles representing the populations over time.
            double[,] data = new double[N, S.N];
            // Initialize the system to have the population specified in the system.
            for (int i = 0; i < S.N; ++i)
                data[0, i] = S.x[i];

            // Run the simulation once first to ensure JIT has ocurred if necessary.
            Simulate(N, dt, data);

            // Start time of the simulation.
            long start = Timer.Counter;
            // run the simulation 100 times to avoid noise.
            for (int i = 0; i < 100; ++i)
                Simulate(N, dt, data);
            double time = Timer.Delta(start);
            Console.WriteLine("{0} time: {1}", Title, time);

            // Plot the resulting data.
            Plot plot = new Plot()
            {
                x0 = 0.0,
                x1 = N,
                Title = Title,
                xLabel = "Timestep",
                yLabel = "Population",
            };
            for (int i = 0; i < S.N; ++i)
            {
                KeyValuePair<double, double>[] points = new KeyValuePair<double, double>[N];
                for (int j = 0; j < N; ++j)
                    points[j] = new KeyValuePair<double, double>(j, data[j, i]);
                plot.Series.Add(new Scatter(points) { Name = i.ToString() });
            }

            return time;
        }

        static void Main(string[] args)
        {
            // The number of timesteps simulated.
            const int N = 100000;
            // Time between timesteps.
            const double dt = 0.001;

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

            // Define a function to run the simulation specifically for S.
            Func<int, double[,], int> SimulateAlgebra = DefineSimulate(dt, S);

            double native = RunSimulation((_N, _dt, _Data) => SimulateNative(_N, _dt, _Data, S), N, dt, S, "Native Simulation");
            double hardcoded = RunSimulation((_N, _dt, _Data) => SimulateNativeHardCoded(_N, _Data), N, dt, S, "Native Simulation (hard-coded)");
            double algebra = RunSimulation((_N, _dt, _Data) => SimulateAlgebra(_N, _Data), N, dt, S, "Algebraic Simulation");

            Console.WriteLine("Algebraic simulation is {0:G3}x faster than native simulation, {1:G3}x faster than hardcoded simulation", native / algebra, hardcoded / algebra);

            // On my machine, the above indicates that the algebraic simulation is 14x faster than native, and ~1.5x faster than the hardcoded simulation.
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
