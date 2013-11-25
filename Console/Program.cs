using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputerAlgebra;
using ComputerAlgebra.LinqCompiler;

namespace Console
{
    class ConsoleNamespace : Namespace
    {
        public ConsoleNamespace() : base(typeof(ConsoleNamespace)) { }

        public override IEnumerable<Expression> LookupName(string Name) 
        {
            return Global.LookupName(Name).Concat(base.LookupName(Name));
        }

        public static void Plot(Expression f, Variable x, Constant x0, Constant x1)
        {
            ComputerAlgebra.Plotting.Plot p = new ComputerAlgebra.Plotting.Plot() { x0 = (double)x0, x1 = (double)x1 };
            foreach (Expression i in Set.MembersOf(f))
            {
                p.Series.Add(new ComputerAlgebra.Plotting.Function(ExprFunction.New(i, x).Compile<Func<double, double>>()));
            }
        }

        public static void Plot(Expression f, Variable x) { Plot(f, x, -10, 10); }
    };

    class Program
    {   
        static void Main(string[] args)
        {
            Parser parser = new Parser(new ConsoleNamespace());

            while (true)
            {
                try
                {
                    System.Console.Write("> ");
                    string s = System.Console.ReadLine();
                    System.Console.WriteLine();

                    if (s == "exit")
                        break;

                    Expression E = parser.Parse(s);

                    System.Console.WriteLine(Arrow.New(E, E.Evaluate()).ToPrettyString());
                    System.Console.WriteLine();
                }
                catch (Exception Ex)
                {
                    System.Console.WriteLine(Ex.Message);
                }
            }
        }
    }
}
