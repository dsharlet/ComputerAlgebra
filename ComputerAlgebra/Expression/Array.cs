namespace ComputerAlgebra
{
    /// <summary>
    /// Represents an arbitrary array.
    /// </summary>
    public class Array : NamedAtom
    {
        private System.Array value;
        public System.Array Value { get { return value; } }

        private Array(string Name, System.Array Value) : base(Name) { value = Value; }

        public static Array New<T>(string Name, int N1) { return new Array(Name, new T[N1]); }
        public static Array New<T>(string Name, int N1, int N2) { return new Array(Name, new T[N1, N2]); }
        public static Array New<T>(string Name, int N1, int N2, int N3) { return new Array(Name, new T[N1, N2, N3]); }
    }
}