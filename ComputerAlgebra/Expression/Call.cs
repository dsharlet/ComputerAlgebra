using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Function call expression.
    /// </summary>
    public class Call : Atom
    {
        protected Function target;
        /// <summary>
        /// Function called by this call expression.
        /// </summary>
        public Function Target { get { return target; } }

        protected IEnumerable<Expression> arguments;
        /// <summary>
        /// Arguments for the call.
        /// </summary>
        public IEnumerable<Expression> Arguments { get { return arguments; } }

        protected Call(Function Target, IEnumerable<Expression> Args) 
        {
            Debug.Assert(!ReferenceEquals(Target, null));
            target = Target; 
            arguments = Args; 
        }

        /// <summary>
        /// Create a new Call expression.
        /// </summary>
        /// <param name="Target">Function to call.</param>
        /// <param name="Args">Arguments for the call.</param>
        /// <returns>Constructed Call expression.</returns>
        public static Call New(Function Target, IEnumerable<Expression> Args) { return new Call(Target, Args.Buffer()); }
        public static Call New(Function Target, params Expression[] Args) { return new Call(Target, Args); }

        /// <summary>
        /// Create a new Call expression to an unknown function.
        /// </summary>
        /// <param name="Target">Name of the function to call.</param>
        /// <param name="Args">Arguments for the call.</param>
        /// <returns>Constructed Call expression.</returns>
        public static Call New(string Target, IEnumerable<Expression> Args) 
        {
            Args = Args.Buffer();
            return new Call(UnknownFunction.New(Target, Args.Count()), Args); 
        }
        public static Call New(string Target, params Expression[] Args) { return new Call(UnknownFunction.New(Target, Args.Length), Args); }

        public override bool Matches(Expression E, MatchContext Matched) { return target.CallMatches(arguments, E, Matched); }

        // object interface.
        public override bool Equals(Expression E) { return target.CallEquals(arguments, E) || base.Equals(E); }
        public override int GetHashCode() { return target.GetHashCode() ^ arguments.OrderedHashCode(); }

        protected override int TypeRank { get { return 2; } }
        public override int CompareTo(Expression R)
        {
            Call RF = R as Call;
            if (!ReferenceEquals(RF, null))
                return LexicalCompareTo(
                    () => target.CompareTo(RF.Target),
                    () => arguments.LexicalCompareTo(RF.Arguments));

            return base.CompareTo(R);
        }

        // Resolve a call to a function Name in the global namespace.
        private static Call CallGlobal(string Name, params Expression[] Args) { return Call.New(Namespace.Global.Resolve(Name, Args), Args); }

        public static Expression If(Expression c, Expression t, Expression f) { return CallGlobal("If", c, t, f); }

        public static Expression DependsOn(Expression f, Expression x) { return CallGlobal("DependsOn", f, x); }
        public static Expression IsConstant(Expression x) { return CallGlobal("IsConstant", x); }
        public static Expression IsInteger(Expression x) { return CallGlobal("IsInteger", x); }
        public static Expression IsNatural(Expression x) { return CallGlobal("IsNatural", x); }

        public static Expression Abs(Expression x) { return CallGlobal("Abs", x); }
        public static Expression Sign(Expression x) { return CallGlobal("Sign", x); }

        public static Expression Min(Expression a, Expression b) { return CallGlobal("Min", a, b); }
        public static Expression Max(Expression a, Expression b) { return CallGlobal("Max", a, b); }

        public static Expression Sin(Expression x) { return CallGlobal("Sin", x); }
        public static Expression Cos(Expression x) { return CallGlobal("Cos", x); }
        public static Expression Tan(Expression x) { return CallGlobal("Tan", x); }
        public static Expression Sec(Expression x) { return CallGlobal("Sec", x); }
        public static Expression Csc(Expression x) { return CallGlobal("Csc", x); }
        public static Expression Cot(Expression x) { return CallGlobal("Cot", x); }
        public static Expression ArcSin(Expression x) { return CallGlobal("ArcSin", x); }
        public static Expression ArcCos(Expression x) { return CallGlobal("ArcCos", x); }
        public static Expression ArcTan(Expression x) { return CallGlobal("ArcTan", x); }
        public static Expression ArcSec(Expression x) { return CallGlobal("ArcSec", x); }
        public static Expression ArcCsc(Expression x) { return CallGlobal("ArcCsc", x); }
        public static Expression ArcCot(Expression x) { return CallGlobal("ArcCot", x); }

        public static Expression Sinh(Expression x) { return CallGlobal("Sinh", x); }
        public static Expression Cosh(Expression x) { return CallGlobal("Cosh", x); }
        public static Expression Tanh(Expression x) { return CallGlobal("Tanh", x); }
        public static Expression Sech(Expression x) { return CallGlobal("Sech", x); }
        public static Expression Csch(Expression x) { return CallGlobal("Csch", x); }
        public static Expression Coth(Expression x) { return CallGlobal("Coth", x); }
        public static Expression ArcSinh(Expression x) { return CallGlobal("ArcSinh", x); }
        public static Expression ArcCosh(Expression x) { return CallGlobal("ArcCosh", x); }
        public static Expression ArcTanh(Expression x) { return CallGlobal("ArcTanh", x); }
        public static Expression ArcSech(Expression x) { return CallGlobal("ArcSech", x); }
        public static Expression ArcCsch(Expression x) { return CallGlobal("ArcCsch", x); }
        public static Expression ArcCoth(Expression x) { return CallGlobal("ArcCoth", x); }

        public static Expression Sqrt(Expression x) { return CallGlobal("Sqrt", x); }
        public static Expression Exp(Expression x) { return CallGlobal("Exp", x); }
        public static Expression Ln(Expression x) { return CallGlobal("Ln", x); }
        public static Expression Log(Expression x, Expression b) { return CallGlobal("Log", x, b); }

        public static Expression Factorial(Expression x) { return CallGlobal("Factorial", x); }

        public static Expression Factor(Expression x) { return CallGlobal("Factor", x); }
        public static Expression Expand(Expression x) { return CallGlobal("Expand", x); }

        public static Expression D(Expression f, Expression x) { return CallGlobal("D", f, x); }
        public static Expression I(Expression f, Expression x) { return CallGlobal("I", f, x); }
        public static Expression L(Expression f, Expression t, Expression s) { return CallGlobal("L", f, t, s); }
        public static Expression IL(Expression f, Expression s, Expression t) { return CallGlobal("IL", f, s, t); }
    }
}