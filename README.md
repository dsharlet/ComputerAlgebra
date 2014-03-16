About
-----

This project is a library for enabling computer algebra as a service in .Net applications. It is focused on providing tools to help perform numerical computations efficiently.

A key feature of the project is enabling compilation of expresions resulting from algebraic operations to "native" .Net code via LINQ Expressions compiled to delegates. This allows complex systems to be defined at runtime, the solutions can be found and compiled once, and then the solutions used nearly as efficiently as hand-written .Net coded solutions. In other words, ComputerAlgebra compiled expressions allow programs to have the flexibility of behavior defined at runtime by the user, but retain the performance of hand-written hardcoded solutions to specific problems.

Development of this project is mostly motivated by a specific use case, **LiveSPICE**: http://www.livespice.org. LiveSPICE is a circuit simulation project loosely aimed at replicating the functionality of other SPICE simulations, with the unique feature of being able to run simulations in real time on live audio signals.

ComputerAlgebra is quite early in development. The only functionality that could be considered reliable is that which is used by LiveSPICE. There are many features of ComputerAlgebra that I started working on believing they would be useful for LiveSPICE, but I later abandoned because they proved not to be useful. A good example of this is the DSolve function, which uses the Laplace transform to solve systems of differential equations. While it will probably work for some small problems, it will generally not be reliable. I would certainly appreciate any contributions if you find something missing or broken! At the very least, a bug report would be appreciated :)

Basic Usage
-----------

This section is basically a reproduction of the **Demo** project in the repo. 

Here are some examples of creating expressions:

```csharp
// Create some constants.
Expression A = 2;
Constant B = Constant.New(3);

// Create some variables.
Expression x = "x";
Variable y = Variable.New("y");
            
// Create basic expression with operator overloads.
Expression f = A*x + B*y + 4;

// This expression uses the implicit conversion from string to
// Expression, which parses the string.
Expression g = "5*x + C*y + 8";

// Create a system of equations from the above expressions.
var system = new List<Equal>()
{
    Equal.New(f, 0),
    Equal.New(g, 0),
};
```

Here is an example of solving a system of equations:

```csharp
// We can now solve the system of equations for x and y. Since the
// equations have a variable 'C', the solutions will not be
// constants.
List<Arrow> solutions = system.Solve(x, y);
Console.WriteLine("The solutions are:");
foreach (Arrow i in solutions)
    Console.WriteLine(i.ToString());
```

A fundamental building block of ComputerAlgebra is the `Arrow` expression. Arrow expressions define the value of one expression to be the value given by another expression. For example, `x->2` defines the expression `x` to have the value `2`. `x->2` can be constructed with `Arrow.New(x, 2)` in code.

The `Solve` function used above returns a list of Arrow expressions defining the solutions of the system. 

Arrow expressions are used by the `Evaluate` function to substitute values for expressions into other expressions. To demonstrate the usage of Evaluate, let's validate the solution by using Evaluate to substitute the solutions into the original system of equations, and then substitute a value for C.

```csharp
Expression f_xy = f.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
Expression g_xy = g.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
if ((f_xy == 0) && (g_xy == 0))
    Console.WriteLine("Success!");
else
    Console.WriteLine("Failure! f = {0}, g = {1}", f_xy, g_xy);
```

Suppose we need to evaluate the solutions efficiently many times. We can compile the solutions to delegates where `C` is a parameter to the delegate, allowing it to be specified later.

```csharp
var x_C = x.Evaluate(solutions).Compile<Func<double, double>>("C");
var y_C = y.Evaluate(solutions).Compile<Func<double, double>>("C");

for (int i = 0; i < 20; ++i)
{
    double C = i / 2.0;
    Console.WriteLine("C = {0}: (x, y) = ({1}, {2})", C, x_C(C), y_C(C));
}
```

Performance
-----------

To demonstrate the potential performance advantages of the concept of compiling simulations, the **LotkaVolterra** demo program uses the compilation features of ComputerAlgebra to compile the competitive Lotka-Volterra predator-prey population model. The model is a differential equation, solutions are produced using Euler integration. For more information about the model, see http://en.wikipedia.org/wiki/Competitive_Lotka%E2%80%93Volterra_equations.

Several implementations of the simulation are provided:

* **SimulateNative**: A normal C# implementation. This implementation is fully general, it can support any `PopulationSystem` instance given.
* **SimulateNativeHardCoded**: A C# implementation that is hardcoded to the specific PopulationSystem defined in `Main`.
* **SimulateAlgebra**: This implementation is generated at runtime via ComputerAlgebra simplified expressions. The meat of this implementation is in the `DefineSimulate` function, which generates a function definition for the given PopulationSystem.
* **SimulateNativeHardCoded(C++)**: A C++ implementation of the hardcoded simulation.

On my machine, I get the following timings:

* SimulateNative: **0.68s** (**5.96x** SimulateAlgebra)
* SimulateNativeHardCoded: **0.14s** (**1.22x** SimulateAlgebra)
* **SimulateAlgebra: 0.11s**
* SimulateNativeHardCoded(C++): **0.099s** (**0.86x** SimulateAlgebra)

All simulations should produce identical output, less some subtle differences due to the algebraic manipulations performed by the algebra simulation, which do not necessarily preserve floating point equivalence.

Here are some conclusions we can draw from these results:

* The native simulation is significantly slower than the hardcoded solution or the algebraic solution. This is very likely due to the overhead associated with accessing the simulation parameters via indirection. However, there is no obvious way to significantly reduce this overhead while retaining the full flexibility of the program aside from hardcoding the parameters.
* The hardcoded simulation is much faster, because the overhead of dealing with the simulation logic and parameters is eliminated. However, this simulation is extremely inflexible. Changing any parameters of the simulation requires changing the program itself. This is not practical if you want the simulation behavior to be defined by users.
* The algebraic simulation is faster still, but, it maintains the flexibilty of the general solution. This is because the algebraic expressions describing the simulation are simplified and evaluated as if the simulation were hardcoded. Enabling this performance while maintaining flexibility is the motivation behind the ComputerAlgebra project!
* While we should expect the hardcoded simulation to be the same or slightly faster than the algebraic solution, actually achieving this is not easy. It requires significant error-prone algebraic manipulations to be performed by hand, and I am apparently too sloppy to get it done without making mistakes. Regardless, I believe the conclusions here are supported by the data.
* The C++ simulation time is presented only to provide an indication that the algebra solution can roughly reach the same neighborhood of C++ while retaining full flexibility. I have considered developing a native (x86) code compiler for algebraic expressions which would likely deliver similar performance, but I have not yet felt squeezed by the performance of the existing LINQ compiler.

Finally, I'd like to point out that while this demo does prove some performance advantages, this program uses only the most basic computer algebra functionality available (rearranging expressions, constant folding). This means that the gap between the native and algebraic solvers is probably a low estimate for most applications.

More advanced simulations, such as those using implicit integration methods and more interesting solvers, will find many more use cases for algebraically simplifying expressions, increasing the advantage. In LiveSPICE, disabling the algebraic processing and relying on general numerical solutions results in simulations that are **hundreds** of times slower than those using the full algebraic processing.
