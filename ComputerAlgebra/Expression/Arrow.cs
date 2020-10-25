namespace ComputerAlgebra
{
    /// <summary>
    /// Represents an x -> y expression.
    /// </summary>
    public class Arrow : Binary
    {
        protected Arrow(Expression L, Expression R) : base(Operator.Arrow, L, R) { }
        public static Arrow New(Expression L, Expression R) { return new Arrow(L, R); }
    }
}
