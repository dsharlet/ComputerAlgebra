using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ComputerAlgebra
{
    /// <summary>
    /// Sqrt function, matches x^0.5.
    /// </summary>
    public class SqrtFunction : NativeFunction
    {
        private static MethodInfo SqrtMethod = typeof(SqrtFunction).GetMethod("Sqrt", BindingFlags.Static | BindingFlags.NonPublic);
        private static Variable x = Variable.New("x");

        private Variable[] parameters = new Variable[] { x };
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private static Constant Sqrt(Constant x) { return Real.Sqrt(x); }

        private SqrtFunction() : base("Sqrt", null, SqrtMethod) { }

        private static SqrtFunction instance = new SqrtFunction();
        /// <summary>
        /// Get an instance of the Sqrt function.
        /// </summary>
        /// <returns></returns>
        public static SqrtFunction New() { return instance; }

        private static Expression PowHalf = Power.New(x, 0.5);

        public override bool CallMatches(IEnumerable<Expression> Arguments, Expression E, MatchContext Matched)
        {
            return base.CallMatches(Arguments, E, Matched) || PowHalf.Matches(E, Matched);
        }

        public override bool CallEquals(IEnumerable<Expression> Arguments, Expression E)
        {
            if (base.CallEquals(Arguments, E))
                return true;

            MatchContext m = PowHalf.Matches(E);
            return m != null && m[x].Equals(Arguments.Single());
        }
    }
}
