using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Function defined by an expression.
    /// </summary>
    public class ExprFunction : Function
    {
        private Expression body;
        /// <summary>
        /// Get the body of this function.
        /// </summary>
        public Expression Body { get { return body; } }

        private IEnumerable<Variable> parameters;
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private ExprFunction(string Name, Expression Body, IEnumerable<Variable> Params) : base(Name) 
        {
            if (ReferenceEquals(Body, null))
                throw new ArgumentNullException("Body");

            body = Body; 
            parameters = Params; 
        }

        /// <summary>
        /// Create a new anonymous function defined by an expression.
        /// </summary>
        /// <param name="Body"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static ExprFunction New(Expression Body, IEnumerable<Variable> Params) { return new ExprFunction("<Anonymous>", Body, Params.Buffer()); }
        public static ExprFunction New(Expression Body, params Variable[] Params) { return New(Body, Params.AsEnumerable()); }

        /// <summary>
        /// Create a new named function defined by an expression.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Body"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static ExprFunction New(string Name, Expression Body, IEnumerable<Variable> Params) { return new ExprFunction(Name, Body, Params.Buffer()); }
        public static ExprFunction New(string Name, Expression Body, params Variable[] Params) { return New(Name, Body, Params.AsEnumerable()); }

        public override Expression Call(IEnumerable<Expression> Args) { return body.Evaluate(parameters.Zip(Args, (a, b) => Arrow.New(a, b))); }

        public override bool CanCall(IEnumerable<Expression> Args) { return parameters.Count() == Args.Count(); }
        public override bool CanCall() { return true; }
    }
}
