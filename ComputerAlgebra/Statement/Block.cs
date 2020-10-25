using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents an ordered sequence of child statements.
    /// </summary>
    public class Block : Statement
    {
        private IEnumerable<Statement> statements;
        /// <summary>
        /// Enumerates the sequence of child statements in this block.
        /// </summary>
        public IEnumerable<Statement> Statements { get { return statements; } }

        private Block(IEnumerable<Statement> Statements) { statements = Statements; }

        /// <summary>
        /// Create a new block.
        /// </summary>
        /// <param name="Statements"></param>
        /// <returns></returns>
        public static Block New(IEnumerable<Statement> Statements) { return new Block(Statements.Buffer()); }
        public static Block New(params Statement[] Statements) { return new Block(Statements); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            foreach (Statement i in statements)
                i.Execute(State);
        }
    }
}
