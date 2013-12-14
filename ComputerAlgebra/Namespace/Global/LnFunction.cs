using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace ComputerAlgebra
{
    /// <summary>
    /// Sqrt function, matches x^0.5.
    /// </summary>
    public class LnFunction : NativeFunction
    {
        private static MethodInfo LnMethod = typeof(LnFunction).GetMethod("Ln", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo LogMethod = typeof(GlobalNamespace).GetMethod("Log", BindingFlags.Static | BindingFlags.Public);
        private static Variable x = Variable.New("x");

        private Variable[] parameters = new Variable[] { x };
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private static Constant Ln(Constant x) { return Real.Ln(x); }

        private LnFunction() : base("Ln", null, LnMethod) { }

        private static LnFunction instance = new LnFunction();
        /// <summary>
        /// Get an instance of the Ln function.
        /// </summary>
        /// <returns></returns>
        public static LnFunction New() { return instance; }

        private static Expression LogE = ComputerAlgebra.Call.New(NativeFunction.New(LogMethod), x, Real.e);

        public override bool CallMatches(IEnumerable<Expression> Arguments, Expression E, MatchContext Matched)
        {
            return base.CallMatches(Arguments, E, Matched) || LogE.Matches(E, Matched);
        }

        public override bool CallEquals(IEnumerable<Expression> Arguments, Expression E)
        {
            if (base.CallEquals(Arguments, E))
                return true;

            MatchContext m = LogE.Matches(E);
            return m != null && m[x].Equals(Arguments.Single());
        }
    }
}
