using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a do { body } while(cond) loop.
    /// </summary>
    public class DoWhile : Statement
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

        private DoWhile(Expression Condition, Statement Body) { cond = Condition; body = Body; }

        /// <summary>
        /// Create a new while loop.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static DoWhile New(Expression Condition, Statement Body) { return new DoWhile(Condition, Body); }
        public static DoWhile New(Statement Body) { return new DoWhile(null, Body); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            do
            {
                try
                {
                    body.Execute(State);
                }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            } while (cond.Evaluate(State));
        }
    }
}
