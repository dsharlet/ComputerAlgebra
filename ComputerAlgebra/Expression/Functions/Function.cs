using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace ComputerAlgebra
{
    /// <summary>
    /// Base class for a function.
    /// </summary>
    public abstract class Function : NamedAtom
    {
        protected Function(string Name) : base(Name) { }

        /// <summary>
        /// Enumerate the parameters of this function.
        /// </summary>
        public abstract IEnumerable<Variable> Parameters { get; }

        /// <summary>
        /// Evaluate this function with the given arguments.
        /// </summary>
        /// <param name="Args"></param>
        /// <returns></returns>
        public abstract Expression Call(IEnumerable<Expression> Args);
        public Expression Call(params Expression[] Args) { return Call(Args.AsEnumerable()); }
        
        /// <summary>
        /// Check if this function could be called with the given parameters.
        /// </summary>
        /// <param name="Args"></param>
        /// <returns></returns>
        public virtual bool CanCall(IEnumerable<Expression> Args) { return CanCall() && Parameters.Count() == Args.Count(); }
        public virtual bool CanCall() { return true; }

        /// <summary>
        /// Check if a call to this function with the given arguments matches an expression.
        /// </summary>
        /// <param name="Arguments"></param>
        /// <param name="E"></param>
        /// <param name="Matched"></param>
        /// <returns></returns>
        public virtual bool CallMatches(IEnumerable<Expression> Arguments, Expression E, MatchContext Matched)
        {
            Call EF = E as Call;
            if (!ReferenceEquals(EF, null))
            {
                if (!Equals(EF.Target))
                    return false;
                if (Arguments.Count() != EF.Arguments.Count())
                    return false;

                return Matched.TryMatch(() => Arguments.Reverse().Zip(EF.Arguments.Reverse(), (p, e) => p.Matches(e, Matched)).All());
            }

            return false;
        }

        /// <summary>
        /// Check if a call to this function with the given arguments is equal to an expression.
        /// </summary>
        /// <param name="Arguments"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        public virtual bool CallEquals(IEnumerable<Expression> Arguments, Expression E)
        {
            Call C = E as Call;
            if (ReferenceEquals(C, null)) return false;

            return Equals(C.Target) && Arguments.SequenceEqual(C.Arguments);
        }

        /// <summary>
        /// Substitute the variables into the expressions.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="IsTransform"></param>
        /// <returns></returns>
        public virtual Expression Substitute(Call C, IDictionary<Expression, Expression> x0, bool IsTransform)
        {
            return ComputerAlgebra.Call.New(C.Target, C.Arguments.Select(i => i.Substitute(x0, IsTransform)));
        }
    }
}
