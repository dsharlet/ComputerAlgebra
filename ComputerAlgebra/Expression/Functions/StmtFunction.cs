using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Function defined by an expression.
    /// </summary>
    public class StmtFunction : Function
    {
        private Statement body;
        /// <summary>
        /// Get the body of this function.
        /// </summary>
        public Statement Body { get { return body; } }

        private IEnumerable<Variable> parameters;
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private StmtFunction(string Name, Statement Body, IEnumerable<Variable> Params)
            : base(Name)
        {
            if (ReferenceEquals(Body, null))
                throw new ArgumentNullException("Body");

            body = Body;
            parameters = Params;
        }

        /// <summary>
        /// Create a new anonymous function defined by a statement.
        /// </summary>
        /// <param name="Body"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static StmtFunction New(Statement Body, IEnumerable<Variable> Params) { return new StmtFunction("<Anonymous>", Body, Params.Buffer()); }
        public static StmtFunction New(Statement Body, params Variable[] Params) { return New(Body, Params.AsEnumerable()); }

        /// <summary>
        /// Create a new named function defined by a statement.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Body"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static StmtFunction New(string Name, Statement Body, IEnumerable<Variable> Params) { return new StmtFunction(Name, Body, Params.Buffer()); }
        public static StmtFunction New(string Name, Statement Body, params Variable[] Params) { return New(Name, Body, Params.AsEnumerable()); }

        public override Expression Call(IEnumerable<Expression> Args)
        {
            try
            {
                body.Execute(parameters.Zip(Args, (a, b) => Arrow.New(a, b)));
            }
            catch (ReturnException Ret)
            {
                return Ret.Value;
            }
            throw new InvalidOperationException("Return statement not executed.");
        }
    }
}
