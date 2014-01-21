using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ComputerAlgebra
{
    /// <summary>
    /// If function.
    /// </summary>
    public class IfFunction : Function
    {
        private Variable[] parameters = new Variable[] { Variable.New("c"), Variable.New("t"), Variable.New("f") };
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private IfFunction() : base("If") { }

        private static IfFunction instance = new IfFunction();
        /// <summary>
        /// Get an instance of the If function.
        /// </summary>
        /// <returns></returns>
        public static IfFunction New() { return instance; }

        public override Expression Call(IEnumerable<Expression> Args)
        {
            Expression[] args = Args.ToArray();

            // If both branches are equal, just return one of them as the result.
            if (args[1].Equals(args[2]))
                return args[1];

            // Try to evaluate the condition.
            if (args[0].IsTrue())
                return args[1];
            else if (args[0].IsFalse())
                return args[2];

            // Couldn't evaluate with these arguments.
            return null;
        }
    }
}
