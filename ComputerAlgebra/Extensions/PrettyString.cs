using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    class PrettyString
    {
        private List<string> lines;
        int zero;

        public PrettyString(int ZeroLine, IEnumerable<string> Lines) { lines = Lines.ToList(); zero = ZeroLine; }
        public PrettyString(int ZeroLine, params string[] Lines) { lines = Lines.ToList(); zero = ZeroLine; }

        public IEnumerable<string> Lines { get { return lines; } }
        public int LineCount { get { return lines.Count; } }
        public int ColumnCount { get { return lines.Max(i => i.Length); } }
        public int ZeroRow { get { return zero; } }

        public PrettyString PadLines(int Min, int Max)
        {
            int l0 = Math.Min(-ZeroRow, Min);
            int l1 = Math.Max(LineCount - ZeroRow, Max);

            return new PrettyString(ZeroRow, Enumerable.Repeat("", -ZeroRow - l0).Concat(Lines).Concat(Enumerable.Repeat("", l1 - (LineCount - ZeroRow))));
        }

        public static PrettyString ConcatLines(int Zero, PrettyString A, PrettyString B)
        {
            int cols = Math.Max(A.ColumnCount, B.ColumnCount);
            string padA = new string(' ', (cols - A.ColumnCount) / 2);
            string padB = new string(' ', (cols - B.ColumnCount) / 2);
            return new PrettyString(Zero, A.Lines.Select(i => padA + i).Concat(B.Lines.Select(i => padB + i)));
        }

        public static PrettyString ConcatColumns(PrettyString L, PrettyString R)
        {
            int l0 = Math.Min(-L.ZeroRow, -R.ZeroRow);
            int l1 = Math.Max(L.LineCount - L.ZeroRow, R.LineCount - R.ZeroRow);

            IEnumerable<string> linesL = Enumerable.Repeat("", -L.ZeroRow - l0).Concat(L.Lines).Concat(Enumerable.Repeat("", l1 - (L.LineCount - L.ZeroRow)));
            IEnumerable<string> linesR = Enumerable.Repeat("", -R.ZeroRow - l0).Concat(R.Lines).Concat(Enumerable.Repeat("", l1 - (R.LineCount - R.ZeroRow)));

            int cols = L.ColumnCount;

            return new PrettyString(-l0, linesL.Zip(linesR, (i, j) => i + new string(' ', cols - i.Length) + j));
        }

        public static PrettyString ConcatColumns(PrettyString L, PrettyString M, PrettyString R)
        {
            return ConcatColumns(L, ConcatColumns(M, R));
        }

        public static implicit operator PrettyString(string l) { return new PrettyString(0, l); }

        public override string ToString() { return String.Join("\r\n", lines); }
    }

    class PrettyStringVisitor : ExpressionVisitor<PrettyString>
    {
        private static PrettyString MakeLParen(PrettyString For)
        {
            if (For.LineCount == 1)
                return new PrettyString(For.ZeroRow, "(");
            if (For.LineCount == 2)
                return new PrettyString(For.ZeroRow,
                    "/",
                    "\\");

            return new PrettyString(For.ZeroRow,
                Enumerable.Repeat(@"/", 1).Concat(
                Enumerable.Repeat(@"|", For.LineCount - 2)).Concat(
                Enumerable.Repeat(@"\", 1)));
        }

        private static PrettyString MakeLBrace(PrettyString For)
        {
            if (For.LineCount == 1)
                return new PrettyString(For.ZeroRow, "{");
            if (For.LineCount <= 3)
                return new PrettyString(For.ZeroRow,
                    @"(",
                    @"<",
                    @"(");
            if (For.LineCount <= 5)
                return new PrettyString(For.ZeroRow,
                    @"/",
                    @"\",
                    @"<",
                    @"/",
                    @"\");

            return new PrettyString(For.ZeroRow,
                Enumerable.Repeat(@"/", 1).Concat(
                Enumerable.Repeat(@"\", 1)).Concat(
                Enumerable.Repeat(@"|", (For.LineCount - 5) / 2)).Concat(
                Enumerable.Repeat(@"<", 1)).Concat(
                Enumerable.Repeat(@"|", (For.LineCount - 4) / 2)).Concat(
                Enumerable.Repeat(@"/", 1)).Concat(
                Enumerable.Repeat(@"\", 1)));
        }

        private static PrettyString MakeLBracket(PrettyString For)
        {
            return new PrettyString(For.ZeroRow, Enumerable.Repeat("[", For.LineCount));
        }

        private static Dictionary<string, string> FlipMap = new Dictionary<string, string>() { { "(", ")" }, { "{", "}" }, { "[", "]" }, { "|", "|" }, { "<", ">" }, { "/", "\\" }, { "\\", "/" } };
        private static PrettyString FlipParen(PrettyString x) { return new PrettyString(x.ZeroRow, x.Lines.Select(i => FlipMap[i])); }
        
        private static PrettyString MakeRParen(PrettyString For) { return FlipParen(MakeLParen(For)); }
        private static PrettyString MakeRBrace(PrettyString For) { return FlipParen(MakeLBrace(For)); }
        private static PrettyString MakeRBracket(PrettyString For) { return FlipParen(MakeLBracket(For)); }

        private static PrettyString ConcatParens(PrettyString x) { return PrettyString.ConcatColumns(MakeLParen(x), x, MakeRParen(x)); }
        private static PrettyString ConcatBraces(PrettyString x) { return PrettyString.ConcatColumns(MakeLBrace(x), x, MakeRBrace(x)); }
        private static PrettyString ConcatBrackets(PrettyString x) { return PrettyString.ConcatColumns(MakeLBracket(x), x, MakeRBracket(x)); }

        private static int Precedence(Expression x)
        {
            if (x is Sum)
                return Parser.Precedence(Operator.Add);
            else if (x is Product)
                return Parser.Precedence(Operator.Multiply);
            else if (x is Binary)
                return Parser.Precedence(((Binary)x).Operator);
            else if (x is Unary)
                return Parser.Precedence(((Unary)x).Operator);
            else if (x is Atom)
                return 100;
            return Parser.Precedence(Operator.Equal);
        }

        private static bool IsNegative(Expression x)
        {
            Constant C = Product.TermsOf(x).FirstOrDefault(i => i is Constant) as Constant;
            if (C != null)
                return C.Value < 0;
            return false;
        }

        private PrettyString UnSplit(IEnumerable<Expression> x, string Delim)
        {
            if (x.Any())
            {
                PrettyString unsplit = Visit(x.First());
                foreach (Expression i in x.Skip(1))
                    unsplit = PrettyString.ConcatColumns(unsplit, Delim, Visit(i));
                return unsplit;
            }
            return "";
        }

        private Stack<int> precedence = new Stack<int>();
        public override PrettyString Visit(Expression E)
        {
            if (precedence.Empty())
                precedence.Push(0);

            int p = Precedence(E);
            bool parens = p < precedence.Peek();
            precedence.Push(p);
            PrettyString V = base.Visit(E);
            precedence.Pop();
            if (parens)
                V = ConcatParens(V);
            return V;
        }

        private PrettyString VisitDivide(Expression N, Expression D)
        {
            precedence.Push(0);
            PrettyString NS = Visit(N);
            PrettyString DS = Visit(D);
            precedence.Pop();

            // Note that since there must be less than 2 columns, there can be no precedence issues.
            if (NS.ColumnCount <= 2 && DS.ColumnCount <= 2)
                return PrettyString.ConcatColumns(NS, "/", DS);

            int Cols = Math.Max(NS.ColumnCount, DS.ColumnCount);
            return PrettyString.ConcatLines(NS.LineCount, NS, PrettyString.ConcatLines(0, new string('-', Cols), DS));
        }

        protected override PrettyString VisitProduct(Product M)
        {
            Expression N = Product.Numerator(M);
            Expression D = Product.Denominator(M);

            bool negative = false;
            if (N is Product && IsNegative(N))
            {
                negative = !negative;
                N = -N;
            }
            if (D is Product && IsNegative(D))
            {
                negative = !negative;
                D = -D;
            }

            if (!D.Equals(1))
                return PrettyString.ConcatColumns(negative ? "- " : "", VisitDivide(N, D));
            else if (N is Product)
                return PrettyString.ConcatColumns(negative ? "-" : "", UnSplit(Product.TermsOf(N), "*"));
            else
                return PrettyString.ConcatColumns(negative ? "-" : "", Visit(N));
        }

        protected override PrettyString VisitSum(Sum A)
        {
            PrettyString s = Visit(A.Terms.First());

            foreach (Expression i in A.Terms.Skip(1))
            {
                if (IsNegative(i))
                    s = PrettyString.ConcatColumns(s, " - ", Visit(-i));
                else
                    s = PrettyString.ConcatColumns(s, " + ", Visit(i));
            }
            return s;
        }

        protected override PrettyString VisitMatrix(Matrix A)
        {
            precedence.Push(0);
            PrettyString[,] a = new PrettyString[A.M, A.N];

            for (int i = 0; i < A.M; ++i)
                for (int j = 0; j < A.N; ++j)
                    a[i, j] = Visit(A[i, j]);

            // Pad each rows' entries' height.
            for (int i = 0; i < A.M; ++i)
            {
                int min = Enumerable.Range(0, A.N).Min(j => -a[i, j].ZeroRow);
                int max = Enumerable.Range(0, A.N).Max(j => a[i, j].LineCount - a[i, j].ZeroRow);
                for (int j = 0; j < A.N; ++j)
                    a[i, j] = a[i, j].PadLines(min, max);
            }

            // Assemble the matrix in columns.
            PrettyString s = "";
            for (int j = 0; j < A.N; ++j)
            {
                PrettyString col = a[0, j];
                for (int i = 1; i < A.M; ++i)
                {
                    col = PrettyString.ConcatLines(col.ZeroRow, col, "");
                    col = PrettyString.ConcatLines((col.LineCount + a[i, j].LineCount) / 2, col, a[i, j]);
                }

                s = PrettyString.ConcatColumns(s, j == 0 ? "" : " ", col);
            }

            precedence.Pop();
            return ConcatBrackets(s);
        }

        protected override PrettyString VisitBinary(Binary B)
        {
            return PrettyString.ConcatColumns(Visit(B.Left), Binary.ToString(B.Operator), Visit(B.Right));
        }

        protected override PrettyString VisitSet(Set S)
        {
            precedence.Push(0);
            PrettyString s = ConcatBraces(UnSplit(S.Members, ", "));
            precedence.Pop();
            return s;
        }

        protected override PrettyString VisitUnary(Unary U)
        {
            return PrettyString.ConcatColumns(Unary.ToStringPrefix(U.Operator), Visit(U.Operand), Unary.ToStringPostfix(U.Operator));
        }

        protected override PrettyString VisitCall(Call F)
        {
            precedence.Push(0);
            PrettyString s = PrettyString.ConcatColumns(F.Target.Name, ConcatBrackets(UnSplit(F.Arguments, ", ")));
            precedence.Pop();
            return s;
        }

        protected override PrettyString VisitPower(Power P)
        {
            if (IsNegative(P.Right))
                return VisitDivide(1, P ^ -1);

            PrettyString l = Visit(P.Left);
            PrettyString r = Visit(P.Right);
            r = new PrettyString(r.ZeroRow + 1, r.Lines);
            return PrettyString.ConcatColumns(l, r);
        }

        protected override PrettyString VisitUnknown(Expression E)
        {
            return E.ToString();
        }
    }

    public static class PrettyStringExtension
    {
        private static PrettyStringVisitor formatter = new PrettyStringVisitor();

        public static string ToPrettyString(this Expression x)
        {
            return formatter.Visit(x).ToString();
        }
    }
}
