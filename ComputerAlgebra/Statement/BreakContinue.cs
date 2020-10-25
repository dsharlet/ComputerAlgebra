using System;
using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception thrown when a break statement is encountered.
    /// </summary>
    public class BreakException : Exception { public BreakException() { } }

    /// <summary>
    /// Exception thrown when a continue statement is encountered.
    /// </summary>
    public class ContinueException : Exception { public ContinueException() { } }

    /// <summary>
    /// Represents a break statement.
    /// </summary>
    public class Break : Statement
    {
        private Break() { }

        private static Break instance = new Break();

        /// <summary>
        /// Create a new block.
        /// </summary>
        /// <param name="Statements"></param>
        /// <returns></returns>
        public static Break New() { return instance; }

        public override void Execute(Dictionary<Expression, Expression> State) { throw new BreakException(); }
    }

    /// <summary>
    /// Represents a continue statement.
    /// </summary>
    public class Continue : Statement
    {
        private Continue() { }

        private static Continue instance = new Continue();

        /// <summary>
        /// Create a new block.
        /// </summary>
        /// <param name="Statements"></param>
        /// <returns></returns>
        public static Continue New() { return instance; }

        public override void Execute(Dictionary<Expression, Expression> State) { throw new ContinueException(); }
    }
}
