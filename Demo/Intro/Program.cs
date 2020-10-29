using ComputerAlgebra;
using ComputerAlgebra.LinqCompiler;
using System;
using System.Collections.Generic;

namespace Intro
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create some constants.
            Expression A = 2;
            Constant B = Constant.New(3);

            // Create some variables.
            Expression x = "x";
            Variable y = Variable.New("y");

            // Create a basic expressions.
            Expression f = A * x + B * y + 4;

            // This expression uses the implicit conversion from string to
            // Expression, which parses the string.
            Expression g = "5*x + C*y + 8";

            // Create a system of equations from the above expressions.
            var system = new List<Equal>()
            {
                Equal.New(f, 0),
                Equal.New(g, 0),
            };

            // We can now solve the system of equations for x and y. Since the
            // equations have a variable 'C', the solutions will not be
            // constants.
            List<Arrow> solutions = system.Solve(x, y);
            Console.WriteLine("The solutions are:");
            foreach (Arrow i in solutions)
                Console.WriteLine(i.ToString());

            // A fundamental building block of ComputerAlgebra is the 'Arrow'
            // expression. Arrow expressions define the value of one expression
            // to be the value given by another expression. For example, 'x->2'
            // defines the expression 'x' to have the value '2'.

            // The 'Solve' function used above returns a list of Arrow 
            // expressions defining the solutions of the system. 

            // Arrow expressions are used by the 'Evaluate' function to 
            // substitute values for expressions into other expressions. To
            // demonstrate the usage of Evaluate, let's validate the solution
            // by using Evaluate to substitute the solutions into the original
            // system of equations, and then substitute a value for C.
            Expression f_xy = f.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
            Expression g_xy = g.Evaluate(solutions).Evaluate(Arrow.New("C", 2));
            if ((f_xy == 0) && (g_xy == 0))
                Console.WriteLine("Success!");
            else
                Console.WriteLine("Failure! f = {0}, g = {1}", f_xy, g_xy);

            // Suppose we need to evaluate the solutions efficiently many times.
            // We can compile the solutions to delegates where 'C' is a 
            // parameter to the delegate, allowing it to be specified later.
            var x_C = x.Evaluate(solutions).Compile<Func<double, double>>("C");
            var y_C = y.Evaluate(solutions).Compile<Func<double, double>>("C");

            for (int i = 0; i < 20; ++i)
            {
                double C = i / 2.0;
                Console.WriteLine("C = {0}: (x, y) = ({1}, {2})", C, x_C(C), y_C(C));
            }
        }
    }
}
