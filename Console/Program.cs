using ComputerAlgebra;
using System;
using System.Collections.Generic;

namespace Console
{
    class Program
    {
        public static void Plot(Expression f, Variable x, Constant x0, Constant x1)
        {
            ComputerAlgebra.Plotting.Plot p = new ComputerAlgebra.Plotting.Plot()
            {
                x0 = (double)x0,
                x1 = (double)x1,
                Title = f.ToString(),
                xLabel = x.ToString(),
            };
            foreach (Expression i in Set.MembersOf(f))
                p.Series.Add(new ComputerAlgebra.Plotting.Function(ExprFunction.New(i, x)) { Name = i.ToString() });
        }

        static void Main(string[] args)
        {
            List<KeyValuePair<Expression, Expression>> InOut = new List<KeyValuePair<Expression, Expression>>();

            DynamicNamespace console = new DynamicNamespace();

            console.Add(NativeFunction.New<Action<Expression, Variable, Constant, Constant>>("Plot", (f, x, x0, x1) => Plot(f, x, x0, x1)));
            console.Add(NativeFunction.New<Action<Expression, Variable>>("Plot", (f, x) => Plot(f, x, -10, 10)));
            console.Add(NativeFunction.New<Action>("Clear", () => System.Console.Clear()));
            console.Add(NativeFunction.New<Func<Expression, Expression>>("In", x => InOut[(int)x].Key));
            console.Add(NativeFunction.New<Func<Expression, Expression>>("Out", x => InOut[(int)x].Value));

            Parser parser = new Parser(new NamespaceSet(Namespace.Global, console));

            while (true)
            {
                try
                {
                    System.Console.Write("> ");
                    string s = System.Console.ReadLine();
                    System.Console.WriteLine();

                    if (s == "Exit")
                        break;

                    Expression input = parser.Parse(s);
                    Expression output = input.Evaluate();

                    int n = InOut.Count;

                    InOut.Add(new KeyValuePair<Expression, Expression>(input, output));

                    System.Console.WriteLine(Arrow.New("In[" + n + "]", input).ToPrettyString());
                    System.Console.WriteLine();
                    System.Console.WriteLine(Arrow.New("Out[" + n + "]", output).ToPrettyString());
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
