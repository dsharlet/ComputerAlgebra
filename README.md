About
-----

This project is a library for enabling computer algebra in .Net applications. It is focused on providing tools to help perform numerical computations efficiently. 

A key feature of the project is enabling compilation of expresions resulting from computer algebraic operations to "native" .Net code (via delegates). This allows complex systems to be defined at runtime, the solutions can be found and compiled once, and then the solutions used nearly as efficiently as hand-written .Net coded solutions. In other words, ComputerAlgebra compiled expressions allow programs to have the flexibility of behavior defined at runtime by the user, but retain the performance of hand-written hardcoded solutions to specific problems.

Development of this project is mostly driven by a specific use case, LiveSPICE: http://www.livespice.org. LiveSPICE is a circuit simulation project loosely aimed at replicating the functionality of other SPICE simulations, with the unique feature of being able to run simulations in real time on live audio signals.

This application is a perfect example of where ComputerAlgebra is useful. Circuit behavior is defined by a system of differential algebraic equations. LiveSPICE works by solving this system of equations during simulation startup and compiling the solutions to a function. This function is then called during simulation. Because of the compilation step, many optimization techniques that normally could only be done by a human for a specific circuit, can now be applied automatically for general circuits defined by users. This stack of optimizations enables the simulation to run in real time at audio sample rates, enabling the differentiating feature of LiveSPICE: running circuit simulations on live audio signals in real time.

This application stresses a few particular areas of the computer algebra system, mainly solving large systems of differential algebraic equations that arise from circuits symbolically. There are many areas of the project that I started working on, but abandoned because they turned out to be not useful for solving circuit simulations. A good example of this is the DSolve function, which attempts to solve systems of differential equations via the Laplace transform (which in turn relies on polynomial manipulations such as partial fractions and factoring). These features will work for some problems, but are not likely to be very reliable. I'd be very accepting of pull requests addressing these areas :)

Basic Usage
-----------

This section is basically a reproduction of the 'Intro' project in the repo. 

Here are some examples of creating expressions:

```csharp
// Create some constants.
Expression A = 2;
Constant B = Constant.New(3);

// Create some variables.
Expression x = "x";
Variable y = Variable.New("y");
            
// Create a basic expressions.
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

A fundamental building block of ComputerAlgebra is the 'Arrow' expression. Arrow expressions define the value of one expression to be the value given by another expression. For example, 'x->2' defines the expression 'x' to have the value '2'.

The 'Solve' function used above returns a list of Arrow expressions defining the solutions of the system. 

Arrow expressions are used by the 'Evaluate' function to substitute values for expressions into other expressions. To demonstrate the usage of Evaluate, let's validate the solution by using Evaluate to substitute the solutions into the original system of equations, and then substitute a value for C.

```csharp
Expression f_xy = f.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
Expression g_xy = g.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
if ((f_xy == 0) && (g_xy == 0))
    Console.WriteLine("Success!");
else
    Console.WriteLine("Failure! f = {0}, g = {1}", f_xy, g_xy);
```

Suppose we need to evaluate the solutions efficiently many times. We can compile the solutions to delegates where 'C' is a parameter to the delegate, allowing it to be specified later.

```csharp
var x_C = x.Evaluate(solutions).Compile<Func<double, double>>("C");
var y_C = y.Evaluate(solutions).Compile<Func<double, double>>("C");

for (int i = 0; i < 20; ++i)
{
    double C = i / 2.0;
    Console.WriteLine("C = {0}: (x, y) = ({1}, {2})", C, x_C(C), y_C(C));
}
```
