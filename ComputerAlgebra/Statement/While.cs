using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a while(cond) { body } loop.
    /// </summary>
    public class While : Statement
    {
        private Expression cond;
        /// <summary>
        /// Condition to test.
        /// </summary>
        public Expression Condition { get { return cond; } }

        private Statement body;
        /// <summary>
        /// Body of the loop.
        /// </summary>
        public Statement Body { get { return body; } }

        private While(Expression Condition, Statement Body) { cond = Condition; body = Body; }

        /// <summary>
        /// Create a new while loop.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static While New(Expression Condition, Statement Body) { return new While(Condition, Body); }
        public static While New(Statement Body) { return new While(null, Body); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            while (cond.Evaluate(State))
            {
                try
                {
                    body.Execute(State);
                }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            }
        }
    }
}
