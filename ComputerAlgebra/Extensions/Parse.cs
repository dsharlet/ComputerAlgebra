using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception thrown when an error occurs in parsing an expression.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string m) : base(m) { }
    }

    /// <summary>
    /// A sequence of tokens.
    /// </summary>
    class TokenStream
    {
        static string Escape(params string [] s) 
        {
            StringBuilder S = new StringBuilder();
            foreach (string i in s)
            {
                S.Append("|(");
                foreach (char j in i)
                    S.Append("\\" + j);
                S.Append(")");
            }
            return S.ToString();
        }

        static string Name = @"[a-zA-Z_]\w*";
        static string Literal = @"[-+]?[0-9]*[\.,]?[0-9]+([eE][-+]?[0-9]+)?";
        static Regex token = new Regex(
            "(" + Name + ")|(" + Literal + ")" +
            Escape("==", "=", "!=", ">=", ">", "<=", "<", "~=", "->",
            "+", "-", "*", "/", "^", "'",
            "!", "&", "|", ":",
            ",", "[", "]", "(", ")", "{", "}", "\u221E"));

        List<string> tokens = new List<string>();

        /// <summary>
        /// Tokenize the string s.
        /// </summary>
        /// <param name="s"></param>
        public TokenStream(string s) 
        {
            MatchCollection matches = token.Matches(s);
            foreach (Match m in matches)
                tokens.Add(m.ToString());
        }

        /// <summary>
        /// Get the current token in the stream.
        /// </summary>
        public string Tok 
        { 
            get 
            { 
                if (tokens.Count > 0) 
                    return tokens.First(); 
                else 
                    return "";  
            }
        }

        /// <summary>
        /// Remove the current token from the stream and return it.
        /// </summary>
        /// <returns></returns>
        public string Consume() 
        { 
            string tok = Tok;  
            tokens.RemoveAt(0); 
            return tok;
        }

        /// <summary>
        /// Assert the current token is in Set, and return the token.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string Expect(params string[] Set) 
        { 
            if (Set.Contains(Tok)) 
                return Consume(); 
            else 
                throw new ParseException("Expected " + String.Join(", ", Set.Select(i => "'" + i + "'"))); 
        }

        /// <summary>
        /// Assert there are no more tokens.
        /// </summary>
        public void ExpectEnd() 
        { 
            if (tokens.Any()) 
                throw new ParseException("Expected end"); 
        }
    }
    
    /// <summary>
    /// Implements "precedence climbing": http://www.engr.mun.ca/~theo/Misc/exp_parsing.htm#classic
    /// </summary>
    public class Parser
    {
        private Namespace context = Namespace.Global;
        public Namespace Context { get { return context; } set { context = value; } }

        private CultureInfo culture = CultureInfo.InstalledUICulture;
        public CultureInfo Culture { get { return culture; } set { culture = value; } }

        private TokenStream tokens;

        public Parser(Namespace Context) { context = Context; }

        //Eparser is
        //   var t : Tree
        //   t := Exp( 0 )
        //   expect( end )
        //   return t
        public Expression Parse(string s)
        {
            tokens = new TokenStream(s);
            Expression t = Exp(0);
            tokens.ExpectEnd();
            return t;            
        }
        
        //Exp( p ) is
        //    var t : Tree
        //    t := P
        //    while next is a binary operator and prec(binary(next)) >= p
        //       const op := binary(next)
        //       consume
        //       const q := case associativity(op)
        //                  of Right: prec( op )
        //                     Left:  1+prec( op )
        //       const t1 := Exp( q )
        //       t := mkNode( op, t, t1)
        //    return t
        private Expression Exp(int p)
        {
            Expression l = P();

            Operator op = new Operator();
            while (true)
            {
                if (IsBinaryOperator(tokens.Tok, ref op) && Precedence(op) >= p)
                {
                    tokens.Consume();

                    int q = Precedence(op) + (IsLeftAssociative(op) ? 1 : 0);
                    Expression r = Exp(q);
                    l = Binary.New(op, l, r);
                }
                else if (IsUnaryPostOperator(tokens.Tok, ref op) && Precedence(op) >= p)
                {
                    tokens.Consume();
                    l = Unary.New(op, l);
                }
                else
                {
                    break;
                }
            }
            return l;            
        }

        // Parse a list of expressions.
        private List<Expression> L(string Delim, string Term)
        {
            List<Expression> exprs = new List<Expression>();
            if (tokens.Tok == Term)
                return exprs;
            do
            {
                exprs.Add(Exp(0));
            } while (tokens.Expect(Delim, Term) != Term);
            return exprs;
        }

        //P is
        //    if next is a unary operator
        //         const op := unary(next)
        //         consume
        //         q := prec( op )
        //         const t := Exp( q )
        //         return mkNode( op, t )
        //    else if next = "("
        //         consume
        //         const t := Exp( 0 )
        //         expect ")"
        //         return t
        //    else if next is a v
        //         const t := mkLeaf( next )
        //         consume
        //         return t
        //    else
        //         error
        private Expression P()
        {
            Operator op = new Operator();
            if (tokens.Tok == "+")
            {
                // Skip unary +.
                tokens.Consume();
                return P();
            }
            else if (IsUnaryPreOperator(tokens.Tok, ref op))
            {
                // Unary operator.
                tokens.Consume();
                Expression t = Exp(Precedence(op));
                return Unary.New(op, t);
            }
            else if (tokens.Tok == "(")
            {
                // Group.
                tokens.Consume();
                Expression t = Exp(0);
                tokens.Expect(")");
                return t;
            }
            else if (tokens.Tok == "{")
            {
                // Set.
                tokens.Consume();
                return Set.New(L(",", "}"));
            }
            else if (tokens.Tok == "[")
            {
                // Matrix.
                tokens.Consume();
                List<List<Expression>> entries = new List<List<Expression>>();
                while (tokens.Tok == "[")
                {
                    tokens.Consume();
                    entries.Add(L(",", "]"));
                }
                tokens.Expect("]");

                return Matrix.New(entries);
            }
            else
            {
                string tok = tokens.Consume();
                
                decimal dec = 0;
                double dbl = 0.0;
                if (decimal.TryParse(tok, NumberStyles.Float, culture, out dec))
                    return Constant.New(dec);
                if (double.TryParse(tok, NumberStyles.Float, culture, out dbl))
                    return Constant.New(dbl);
                else if (tok == "True")
                    return Constant.New(true);
                else if (tok == "False")
                    return Constant.New(false);
                else if (tok == "\u221E" || tok == "oo")
                    return Constant.New(Real.Infinity);
                else if (tokens.Tok == "[")
                {
                    // Bracket function call.
                    tokens.Consume();
                    List<Expression> args = L(",", "]");
                    return Call.New(Resolve(tok, args), args);
                }
                else if (tokens.Tok == "(")
                {
                    // Paren function call.
                    tokens.Consume();
                    List<Expression> args = L(",", ")");
                    return Call.New(Resolve(tok, args), args);
                }
                else
                {
                    return Resolve(tok);
                }
            }
        }

        private Function Resolve(string Token, IEnumerable<Expression> Args)
        {
            Function resolve = context.LookupFunction(Token, Args).SingleOrDefault();
            if (!ReferenceEquals(resolve, null))
                return resolve;
            else
                return UnknownFunction.New(Token, Args.Count());
        }

        private Expression Resolve(string Token)
        {
            Expression resolve = context.LookupName(Token).SingleOrDefault();
            if (!ReferenceEquals(resolve, null))
                return resolve;
            else
                return Variable.New(Token);
        }
        
        static bool IsBinaryOperator(string tok, ref Operator op)
        {
            switch (tok)
            {
                case "+": op = Operator.Add; return true;
                case "-": op = Operator.Subtract; return true;
                case "*": op = Operator.Multiply; return true;
                case "/": op = Operator.Divide; return true;
                case "^": op = Operator.Power; return true;
                case "&": op = Operator.And; return true;
                case "|": op = Operator.Or; return true;
                case ":": op = Operator.Substitute; return true;
                case "->": op = Operator.Arrow; return true;
                case "==": op = Operator.Equal; return true;
                case "!=": op = Operator.NotEqual; return true;
                case ">": op = Operator.Greater; return true;
                case "<": op = Operator.Less; return true;
                case ">=": op = Operator.GreaterEqual; return true;
                case "<=": op = Operator.LessEqual; return true;
                case "~=": op = Operator.ApproxEqual; return true;
                default: return false;
            }
        }

        static bool IsUnaryPreOperator(string tok, ref Operator op)
        {
            switch (tok)
            {
                case "-": op = Operator.Negate; return true;
                case "!": op = Operator.Not; return true;
                default: return false;
            }
        }

        static bool IsUnaryPostOperator(string tok, ref Operator op)
        {
            switch (tok)
            {
                case "'": op = Operator.Prime; return true;
                default: return false;
            }
        }

        public static int Precedence(Expression x)
        {
            if (x is Sum)
                return Precedence(Operator.Add);
            else if (x is Product)
                return Precedence(Operator.Multiply);
            else if (x is Binary)
                return Precedence(((Binary)x).Operator);
            else if (x is Unary)
                return Precedence(((Unary)x).Operator);
            else if (x is Atom)
                return 100;
            return Precedence(Operator.Equal);
        }

        public static int Precedence(Operator op)
        {
            switch (op)
            {
                case Operator.And:
                case Operator.Not:
                    return 3;
                case Operator.Or:
                    return 4;
                case Operator.Add:
                case Operator.Subtract:
                    return 5;
                case Operator.Negate:
                    return 6;
                case Operator.Multiply:
                case Operator.Divide:
                    return 7;
                case Operator.Power:
                    return 8;
                case Operator.Prime:
                    return 9;
                case Operator.Arrow:
                    return 2;
                default:
                    return 1;
            }
        }

        private static bool IsLeftAssociative(Operator op)
        {
            switch (op)
            {
                case Operator.Power: return false;
                default: return true;
            }
        }

    }
}
