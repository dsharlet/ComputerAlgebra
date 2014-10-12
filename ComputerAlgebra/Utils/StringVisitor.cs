using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    class StringVisitor : ExpressionVisitor<string>
    {
        string numberFormat;
        IFormatProvider numberFormatProvider;
        
        public string NumberFormat { get { return numberFormat; } set { numberFormat = value; } }
        public IFormatProvider NumberFormatProvider { get { return numberFormatProvider; } set { numberFormatProvider = value; } }

        protected static bool IsNegative(Expression x)
        {
            Constant C = Product.TermsOf(x).FirstOrDefault(i => i is Constant) as Constant;
            if (C != null)
                return (Real)C < 0;
            return false;
        }

        protected string Join(IEnumerable<Expression> x, string Delim)
        {
            if (x.Any())
            {
                string unsplit = Visit(x.First());
                foreach (Expression i in x.Skip(1))
                    unsplit = unsplit + Delim + Visit(i);
                return unsplit;
            }
            return "";
        }

        protected string Visit(Expression E, int Precedence)
        {
            string v = Visit(E);
            if (Parser.Precedence(E) < Precedence)
                v = "(" + v + ")";
            return v;
        }

        protected override string VisitProduct(Product P)
        {
            int pr = Parser.Precedence(Operator.Multiply);

            int sign = 1;
            StringBuilder s = new StringBuilder();
            foreach (Expression i in P.Terms)
            {
                if (i.Equals(-1))
                {
                    sign *= -1;
                }
                else
                {
                    if (s.Length > 0)
                        s.Append('*');
                    s.Append(Visit(i, pr));
                }
            }
            if (sign == -1)
                return "-" + s.ToString();
            else
                return s.ToString();
        }

        protected override string VisitSum(Sum S)
        {
            int pr = Parser.Precedence(Operator.Add);

            StringBuilder s = new StringBuilder();
            s.Append(Visit(S.Terms.First()));
            foreach (Expression i in S.Terms.Skip(1))
            {
                string si = Visit(i, pr);
                string nsi = Visit(-i, pr);
                if (si.Length < nsi.Length)
                    s.Append(" + " + si);
                else
                    s.Append(" - " + nsi);
            }
            return s.ToString();
        }

        protected override string VisitMatrix(Matrix m)
        {
            StringBuilder s = new StringBuilder();

            s.Append("[");
            for (int i = 0; i < m.M; ++i)
            {
                s.Append("[");
                for (int j = 0; j < m.N; ++j)
                {
                    if (j > 0) s.Append(", ");
                    s.Append(m[i, j].ToString());
                }
                s.Append("]");
            }
            s.Append("]");

            return s.ToString();
        }

        protected override string VisitIndex(Index I)
        {
            return Visit(I.Target) + "[" + Join(I.Indices, ", ") + "]";
        }

        protected override string VisitBinary(Binary B)
        {
            int pr = Parser.Precedence(B.Operator);
            return Visit(B.Left, pr) 
                + Binary.ToString(B.Operator) 
                + Visit(B.Right, pr);
        }

        protected override string VisitSet(Set S)
        {
            IEnumerable<Expression> members = S.Members;
            // If there are more than 100 elements in the set, truncate all but 5 elements and append '...'.
            if (members.Skip(100).Any())
                members = members.Take(5).Append(Variable.New("..."));

            string s = "{" + Join(members, ", ") + "}";
            return s;
        }

        protected override string VisitUnary(Unary U)
        {
            int pr = Parser.Precedence(U.Operator);
            return Unary.ToStringPrefix(U.Operator) 
                + Visit(U.Operand, pr) 
                + Unary.ToStringPostfix(U.Operator);
        }

        protected override string VisitCall(Call F)
        {
            string s = F.Target.Name + "[" + Join(F.Arguments, ", ") + "]";
            return s;
        }

        protected override string VisitVariable(Variable V)
        {
            return V.Name;
        }

        protected override string VisitConstant(Constant C)
        {
            return C.Value.ToString(NumberFormat, NumberFormatProvider);
        }

        protected override string VisitUnknown(Expression E)
        {
            throw new NotImplementedException("ToString(" + E.GetType().ToString() + ")");
        }
    }
}
