using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents an MxN matrix.
    /// </summary>
    public class Matrix : Atom, IEnumerable<Expression>
    {
        protected Expression[,] m;

        /// <summary>
        /// Access a matrix element.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public Expression this[int i, int j] { get { return m[i, j]; } }

        /// <summary>
        /// Access an element of a vector.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Expression this[int i]
        {
            get
            {
                if (M == 1)
                    return m[0, i];
                else if (N == 1)
                    return m[i, 0];
                else
                    throw new InvalidOperationException("Matrix is not a vector");
            }
        }

        private Matrix(int M, int N)
        {
            m = new Expression[M, N];
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    m[i, j] = 0;
        }

        private Matrix(int N)
        {
            m = new Expression[N, N];
            for (int i = 0; i < N; ++i)
                for (int j = 0; j < N; ++j)
                    m[i, j] = i == j ? 1 : 0;
        }

        private Matrix(Matrix A)
        {
            m = new Expression[A.M, A.N];
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    m[i, j] = A.m[i, j];
        }

        /// <summary>
        /// Create an arbitrary MxN matrix.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="N"></param>
        /// <param name="Elements"></param>
        /// <returns></returns>
        public static Matrix New(int M, int N, IEnumerable<Expression> Elements)
        {
            Matrix A = new Matrix(M, N);

            using (IEnumerator<Expression> e = Elements.GetEnumerator())
                for (int i = 0; i < M; ++i)
                    for (int j = 0; j < N; ++j, e.MoveNext())
                        A.m[i, j] = e.Current;

            return A;
        }
        public static Matrix New(int N, IEnumerable<Expression> Elements) { return New(N, N, Elements); }

        /// <summary>
        /// Create an identity matrix.
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        public static Matrix New(int N) { return new Matrix(N); }

        /// <summary>
        /// Create an NxN identity matrix.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="N"></param>

        public int M { get { return m.GetLength(0); } }
        public int N { get { return m.GetLength(1); } }


        /// <summary>
        /// Extract a row from the matrix.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Matrix Row(int i)
        {
            Matrix R = new Matrix(1, N);
            for (int j = 0; j < N; ++j)
                R.m[0, j] = m[i, j];
            return R;
        }

        /// <summary>
        /// Extract a column from the matrix.
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        public Matrix Column(int j)
        {
            Matrix C = new Matrix(M, 1);
            for (int i = 0; i < M; ++i)
                C.m[i, 0] = m[i, j];
            return C;
        }

        /// <summary>
        /// Enumerate the rows of the matrix.
        /// </summary>
        public IEnumerable<Matrix> Rows { get { for (int i = 0; i < M; ++i) yield return Row(i); } }
        /// <summary>
        /// Enumerate the columns of the matrix.
        /// </summary>
        public IEnumerable<Matrix> Columns { get { for (int j = 0; j < N; ++j) yield return Column(j); } }

        // IEnumerator<Expression> interface.
        private class Enumerator : IEnumerator<Expression>, IEnumerator
        {
            private Matrix a;
            private int i = 0;
            private int j = 0;

            object IEnumerator.Current { get { return a[i, j]; } }
            Expression IEnumerator<Expression>.Current { get { return a[i, j]; } }

            public Enumerator(Matrix A) { a = A; }

            public void Dispose() { }
            public bool MoveNext()
            {
                if (++j >= a.N)
                {
                    j = 0;
                    if (++i >= a.M)
                        return false;
                }
                return true;
            }
            public void Reset() { i = 0; j = 0; }
        }
        IEnumerator<Expression> IEnumerable<Expression>.GetEnumerator() { return new Enumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();

            SB.Append("[");
            for (int i = 0; i < M; ++i)
            {
                SB.Append("[");
                for (int j = 0; j < N; ++j)
                {
                    if (j > 0) SB.Append(", ");
                    SB.Append(m[i, j].ToString());
                }
                SB.Append("]");
            }
            SB.Append("]");

            return SB.ToString();
        }

        public override int GetHashCode()
        {
            int hash = 33;
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    hash = m[i, j].GetHashCode() + 33 * hash;
            return hash;
        }

        public override bool Equals(Expression E)
        {
            Matrix A = E as Matrix;
            if (ReferenceEquals(M, null))
                return base.Equals(E);

            if (M != A.M || N != A.N)
                return false;

            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    if (!m[i, j].Equals(A.m[i, j]))
                        return false;

            return true;
        }

        public override bool Matches(Expression Expr, MatchContext Matched)
        {
            Matrix A = Expr as Matrix;
            if (ReferenceEquals(A, null))
                return false;

            if (M != A.M || N != A.N)
                return false;

            return Matched.TryMatch(() =>
            {
                for (int i = 0; i < M; ++i)
                    for (int j = 0; j < N; ++j)
                        if (!m[i, j].Matches(A.m[i, j], Matched))
                            return false;

                return true;
            });
        }

        protected override int TypeRank { get { return 4; } }

        private static void SwapRows(Matrix A, int i1, int i2)
        {
            if (i1 == i2)
                return;

            int N = A.N;
            for (int j = 0; j < N; ++j)
            {
                Expression t = A.m[i1, j];
                A.m[i1, j] = A.m[i2, j];
                A.m[i2, j] = t;
            }
        }
        private static void ScaleRow(Matrix A, int i, Expression s)
        {
            int N = A.N;
            for (int j = 0; j < N; ++j)
                A.m[i, j] *= s;
        }
        private static void ScaleAddRow(Matrix A, int i1, Expression s, int i2)
        {
            int N = A.N;
            for (int j = 0; j < N; ++j)
                A.m[i2, j] += A.m[i1, j] * s;
        }

        public static Matrix operator ^(Matrix A, int B)
        {
            if (A.M != A.N)
                throw new ArgumentException("Non-square matrix");

            int N = A.N;
            if (B < 0)
            {
                Matrix A_ = new Matrix(A);
                Matrix Inv = new Matrix(N);

                // Gaussian elimination, .m[ A I ] ~ .m[ I, A^-1 ]
                for (int i = 0; i < N; ++i)
                {
                    // Find pivot row.
                    int p;
                    for (p = i; p < N; ++p)
                        if (!A_.m[p, i].EqualsZero())
                            break;
                    if (p >= N)
                        throw new ArgumentException("Singular matrix");

                    // Swap pivot row with row i.
                    SwapRows(A_, i, p);
                    SwapRows(Inv, i, p);

                    // Put a 1 in the pivot position.
                    Expression s = 1 / A_.m[i, i];
                    ScaleRow(A_, i, s);
                    ScaleRow(Inv, i, s);

                    // Zero the pivot column elsewhere.
                    for (p = 0; p < N; ++p)
                    {
                        if (i != p)
                        {
                            Expression a = -A_.m[p, i];
                            ScaleAddRow(A_, i, a, p);
                            ScaleAddRow(Inv, i, a, p);
                        }
                    }
                }
                return Inv ^ -B;
            }

            if (B != 1)
                throw new ArgumentException("Unsupported matrix exponent");

            return A;
        }

        public static Matrix operator *(Matrix A, Matrix B)
        {
            if (A.N != B.M)
                throw new ArgumentException("Invalid matrix multiply");
            int M = A.M;
            int N = A.N;
            int L = B.N;

            Matrix AB = new Matrix(M, L);
            for (int i = 0; i < M; ++i)
            {
                for (int j = 0; j < L; ++j)
                {
                    Expression ABij = 0;
                    for (int k = 0; k < N; ++k)
                        ABij += A.m[i, k] * B.m[k, j];
                    AB.m[i, j] = ABij;
                }
            }
            return AB;
        }
        public static Matrix operator *(Matrix A, Expression B)
        {
            int M = A.M, N = A.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A.m[i, j] * B;
            return AB;
        }
        public static Matrix operator *(Expression A, Matrix B)
        {
            int M = B.M, N = B.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A * B.m[i, j];
            return AB;
        }

        public static Matrix operator +(Matrix A, Matrix B)
        {
            if (A.M != B.M || A.N != B.N)
                throw new ArgumentException("Invalid matrix addition");

            int M = A.M, N = A.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A.m[i, j] + B.m[i, j];
            return AB;
        }
        public static Matrix operator +(Matrix A, Expression B)
        {
            int M = A.M, N = A.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A.m[i, j] + B;
            return AB;
        }
        public static Matrix operator +(Expression A, Matrix B)
        {
            int M = B.M, N = B.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A + B.m[i, j];
            return AB;
        }

        public static Matrix operator -(Matrix A, Matrix B)
        {
            if (A.M != B.M || A.N != B.N)
                throw new ArgumentException("Invalid matrix addition");

            int M = A.M, N = A.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A.m[i, j] - B.m[i, j];
            return AB;
        }
        public static Matrix operator -(Matrix A, Expression B)
        {
            int M = A.M, N = A.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A.m[i, j] - B;
            return AB;
        }
        public static Matrix operator -(Expression A, Matrix B)
        {
            int M = B.M, N = B.N;
            Matrix AB = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    AB.m[i, j] = A - B.m[i, j];
            return AB;
        }

        public static Matrix operator -(Matrix A)
        {
            int M = A.M, N = A.N;
            Matrix nA = new Matrix(M, N);
            for (int i = 0; i < M; ++i)
                for (int j = 0; j < N; ++j)
                    nA.m[i, j] = -A.m[i, j];
            return nA;
        }
    }
}