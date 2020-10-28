using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Recursively visit an expression.
    /// </summary>
    public class RecursiveExpressionVisitor : ExpressionVisitor<Expression>
    {
        protected override Expression VisitUnknown(Expression E) { return E; }

        protected virtual IEnumerable<Expression> VisitList(IEnumerable<Expression> List)
        {
            List<Expression> list = new List<Expression>();
            bool equal = true;
            foreach (Expression i in List)
            {
                Expression Vi = Visit(i);
                if (Vi is null) 
                    return null;
                list.Add(Vi);

                equal = equal && ReferenceEquals(Vi, i);
            }
            return equal ? List : list;
        }

        protected override Expression VisitBinary(Binary B)
        {
            Expression L = Visit(B.Left);
            Expression R = Visit(B.Right);
            if (L is null || R is null)
                return null;

            if (ReferenceEquals(L, B.Left) && ReferenceEquals(R, B.Right))
                return B;
            else
                return Binary.New(B.Operator, L, R);
        }

        protected override Expression VisitUnary(Unary U)
        {
            Expression O = Visit(U.Operand);
            if (O is null) 
                return null;

            if (ReferenceEquals(O, U.Operand))
                return U;
            else
                return Unary.New(U.Operator, O);
        }

        protected override Expression VisitSum(Sum A)
        {
            IEnumerable<Expression> terms = VisitList(A.Terms);
            if (terms is null)
                return null;
            return ReferenceEquals(terms, A.Terms) ? A : Sum.New(terms);
        }

        protected override Expression VisitProduct(Product M)
        {
            IEnumerable<Expression> terms = VisitList(M.Terms);
            if (terms is null) 
                return null;
            return ReferenceEquals(terms, M.Terms) ? M : Product.New(terms);
        }

        protected override Expression VisitSet(Set S)
        {
            IEnumerable<Expression> members = VisitList(S.Members);
            if (members is null)
                return null;
            return ReferenceEquals(members, S.Members) ? S : Set.New(members);
        }

        protected override Expression VisitCall(Call F)
        {
            IEnumerable<Expression> arguments = VisitList(F.Arguments);
            if (arguments is null)
                return null;
            return ReferenceEquals(arguments, F.Arguments) ? F : Call.New(F.Target, arguments);
        }

        protected override Expression VisitMatrix(Matrix A)
        {
            IEnumerable<Expression> elements = VisitList(A);
            if (elements is null)
                return null;
            return ReferenceEquals(elements, A) ? A : Matrix.New(A.M, A.N, elements);
        }

        protected override Expression VisitIndex(Index I)
        {
            IEnumerable<Expression> indices = VisitList(I.Indices);
            if ((indices is null)) return null;
            return ReferenceEquals(indices, I.Indices) ? I : Index.New(I.Target, indices);
        }
    }
}
