using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a for({ init }, cond, { next }) { body } loop.
    /// </summary>
    public class For : Statement
    {
        private Statement init;
        /// <summary>
        /// Initialization statement.
        /// </summary>
        public Statement Init { get { return init; } }

        private Expression cond;
        /// <summary>
        /// Condition to test.
        /// </summary>
        public Expression Condition { get { return cond; } }

        private Statement step;
        /// <summary>
        /// Statement executed after each loop iteration.
        /// </summary>
        public Statement Step { get { return step; } }

        private Statement body;
        /// <summary>
        /// Body of the loop.
        /// </summary>
        public Statement Body { get { return body; } }

        private For(Statement Init, Expression Condition, Statement Step, Statement Body)
        {
            init = Init;
            cond = Condition;
            step = Step;
            body = Body;
        }

        /// <summary>
        /// Create a new for loop.
        /// </summary>
        /// <param name="Init"></param>
        /// <param name="Condition"></param>
        /// <param name="Step"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static For New(Statement Init, Expression Condition, Statement Step, Statement Body) { return new For(Init, Condition, Step, Body); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            for (init.Execute(State); cond.Evaluate(State); step.Execute(State))
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
