using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exp function, matches e^x.
    /// </summary>
    public class ExpFunction : NativeFunction
    {
        private static Variable x = Variable.New("x");

        private Variable[] parameters = new Variable[] { x };
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private static Constant Exp(Constant x) { return Real.Exp(x); }
        private static MethodInfo ExpMethod = typeof(ExpFunction).GetMethod("Exp", BindingFlags.Static | BindingFlags.NonPublic);

        private ExpFunction() : base("Exp", null, ExpMethod) { }

        private static ExpFunction instance = new ExpFunction();
        /// <summary>
        /// Get an instance of the Exp function.
        /// </summary>
        /// <returns></returns>
        public static ExpFunction New() { return instance; }

        private static Expression ex = Power.New(Real.e, x);

        public override bool CallMatches(IEnumerable<Expression> Arguments, Expression E, MatchContext Matched)
        {
            return base.CallMatches(Arguments, E, Matched) || ex.Matches(E, Matched);
        }

        public override bool CallEquals(IEnumerable<Expression> Arguments, Expression E)
        {
            if (base.CallEquals(Arguments, E))
                return true;

            MatchContext m = ex.Matches(E);
            return m != null && m[x].Equals(Arguments.Single());
        }
    }
}
