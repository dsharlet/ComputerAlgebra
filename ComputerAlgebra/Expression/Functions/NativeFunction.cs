using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Attribute for disallowing substition through a function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class NoSubstitute : Attribute
    {
        public NoSubstitute() { }
    }
    
    /// <summary>
    /// Function defined by a native function.
    /// </summary>
    public class NativeFunction : Function
    {
        private object _this = null;
        public object This { get { return _this; } }

        private MethodInfo method;
        public MethodInfo Method { get { return method; } }

        public override IEnumerable<Variable> Parameters { get { return Method.GetParameters().Select(i => Variable.New(i.Name)); } }

        protected NativeFunction(string Name, object This, MethodInfo Method)
            : base(Name) 
        {
            _this = This;
            method = Method; 
        }

        public static NativeFunction New(MethodInfo Method) { return new NativeFunction(Method.Name, null, Method); }
        public static NativeFunction New(object This, MethodInfo Method) { return new NativeFunction(Method.Name, This, Method); }
        public static NativeFunction New(string Name, object This, MethodInfo Method) { return new NativeFunction(Name, This, Method); }
        public static NativeFunction New(string Name, Delegate Method) { return new NativeFunction(Name, Method, Method.GetMethodInfo()); }
        public static NativeFunction New<T>(string Name, T Method) { return new NativeFunction(Name, Method, typeof(T).GetMethod("Invoke")); }

        public override Expression Call(IEnumerable<Expression> Args)
        {
            if (!Args.Zip(Method.GetParameters(), (a, p) => p.ParameterType.IsAssignableFrom(a.GetType())).All())
                return null;

            try
            {
                object ret = Method.Invoke(_this, Args.ToArray<object>());
                if (ret is Expression)
                    return ret as Expression;
                else
                    return Constant.New(ret);
            }
            catch (TargetInvocationException Ex)
            {
                throw Ex.InnerException;
            }
        }

        public override bool CanCall(IEnumerable<Expression> Args)
        {
            return Method.GetParameters().Length == Args.Count();
        }
        
        public override Expression Substitute(Call C, IDictionary<Expression, Expression> x0, bool IsTransform)
        {
            if (IsTransform)
                return base.Substitute(C, x0, IsTransform);

            Dictionary<Expression, Expression> now = new Dictionary<Expression, Expression>(x0);
            List<Arrow> late = new List<Arrow>();

            foreach (var i in method.GetParameters().Zip(C.Arguments, (p, a) => new { p, a }))
            {
                if (i.p.GetCustomAttribute<NoSubstitute>() != null)
                {
                    if (now.ContainsKey(i.a))
                    {
                        late.Add(Arrow.New(i.a, now[i.a]));
                        now.Remove(i.a);
                    }
                }
            }

            if (!now.Empty())
                C = ComputerAlgebra.Call.New(C.Target, C.Arguments.Select(i => i.Substitute(now)));

            if (late.Empty())
                return C;
            else
                return ComputerAlgebra.Substitute.New(C, late.Count > 1 ? (Expression)Set.New(late) : late.Single());
        }
        
        public override bool Equals(Expression E)
        {
            NativeFunction F = E as NativeFunction;
            if (ReferenceEquals(F, null)) return false;

            return method.Equals(F.method);
        }
        public override int GetHashCode() { return method.GetHashCode(); }
    }
}
