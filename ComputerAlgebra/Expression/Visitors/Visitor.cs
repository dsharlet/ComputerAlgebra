namespace ComputerAlgebra
{
    /// <summary>
    /// Visits an expression.
    /// </summary>
    /// <typeparam name="T">Result type of the visitor.</typeparam>
    public abstract class ExpressionVisitor<T>
    {
        protected abstract T VisitUnknown(Expression E);

        protected virtual T VisitSum(Sum A) { return VisitUnknown(A); }
        protected virtual T VisitProduct(Product M) { return VisitUnknown(M); }
        protected virtual T VisitConstant(Constant C) { return VisitUnknown(C); }
        protected virtual T VisitVariable(Variable V) { return VisitUnknown(V); }
        protected virtual T VisitSet(Set S) { return VisitUnknown(S); }
        protected virtual T VisitBinary(Binary B) { return VisitUnknown(B); }
        protected virtual T VisitUnary(Unary U) { return VisitUnknown(U); }
        protected virtual T VisitPower(Power P) { return VisitBinary(P); }
        protected virtual T VisitCall(Call F) { return VisitUnknown(F); }
        protected virtual T VisitMatrix(Matrix A) { return VisitUnknown(A); }
        protected virtual T VisitIndex(Index I) { return VisitUnknown(I); }

        public virtual T Visit(Expression E)
        {
            if (E is Sum sum) return VisitSum(sum);
            else if (E is Product product) return VisitProduct(product);
            else if (E is Constant constant) return VisitConstant(constant);
            else if (E is Variable variable) return VisitVariable(variable);
            else if (E is Set set) return VisitSet(set);
            else if (E is Power power) return VisitPower(power);
            else if (E is Binary binary) return VisitBinary(binary);
            else if (E is Unary unary) return VisitUnary(unary);
            else if (E is Call call) return VisitCall(call);
            else if (E is Matrix matrix) return VisitMatrix(matrix);
            else if (E is Index index) return VisitIndex(index);
            else return VisitUnknown(E);
        }
    }
}
