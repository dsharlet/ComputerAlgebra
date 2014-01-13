using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception thrown when a return statement is encountered.
    /// </summary>
    public class ReturnException : Exception
    {
        private Expression value;
        public Expression Value { get { return value; } }

        public ReturnException(Expression Value) { value = Value; }
    }

    /// <summary>
    /// Represents an ordered sequence of child statements.
    /// </summary>
    public class Return : Statement
    {
        private Expression value;
        public Expression Value { get { return value; } }

        private Return(Expression Value) { value = Value; }

        /// <summary>
        /// Create a new block.
        /// </summary>
        /// <param name="Statements"></param>
        /// <returns></returns>
        public static Return New(Expression Value) { return new Return(Value); }
        public static Return New() { return new Return(null); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            throw new ReturnException(Value.Evaluate(State));
        }
    }
}
