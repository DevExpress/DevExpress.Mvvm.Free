using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NETFX_CORE
using DevExpress.Mvvm.Native;
#endif

#if MVVM
namespace DevExpress.Mvvm.Native {
#else
namespace DevExpress.Internal {
#endif
#if MVVM
    public
#endif
    static class ExpressionHelper {
        internal static MethodInfo GetArgumentMethodStrict<T>(Expression<Action<T>> expression) {
            return GetArgumentMethodStrictCore(expression);
        }
        internal static MethodInfo GetArgumentFunctionStrict<T, TResult>(Expression<Func<T, TResult>> expression) {
            return GetArgumentMethodStrictCore(expression);
        }
        static MethodInfo GetArgumentMethodStrictCore(LambdaExpression expression) {
            MethodCallExpression memberExpression = GetMethodCallExpression(expression);
            CheckParameterExpression(memberExpression.Object);
            return memberExpression.Method;
        }
        internal static PropertyInfo GetArgumentPropertyStrict<T, TResult>(Expression<Func<T, TResult>> expression) {
            MemberExpression memberExpression = null;
            if(expression.Body is MemberExpression) {
                memberExpression = (MemberExpression)expression.Body;
            } else if(expression.Body is UnaryExpression) {
                UnaryExpression uExp = (UnaryExpression)expression.Body;
                if(uExp.NodeType == ExpressionType.Convert)
                    memberExpression = (MemberExpression)uExp.Operand;
            }
            if(memberExpression == null)
                throw new ArgumentException("expression");
            CheckParameterExpression(memberExpression.Expression);
            return (PropertyInfo)memberExpression.Member;
        }
        static void CheckParameterExpression(Expression expression) {
            if (expression.NodeType == ExpressionType.Parameter)
                return;
            if (expression.NodeType == ExpressionType.Convert) {
                if (((UnaryExpression)expression).Operand.NodeType == ExpressionType.Parameter)
                    return;
            }
            throw new ArgumentException("expression");
        }

        internal static ConstructorInfo GetConstructor<T>(Expression<Func<T>> commandMethodExpression) {
            return GetConstructorCore(commandMethodExpression);
        }
        internal static ConstructorInfo GetConstructorCore(LambdaExpression commandMethodExpression) {
            NewExpression newExpression = commandMethodExpression.Body as NewExpression;
            if(newExpression == null) {
                throw new ArgumentException("commandMethodExpression");
            }
            return newExpression.Constructor;
        }

        public static string GetMethodName(Expression<Action> expression) {
            return GetMethod(expression).Name;
        }
        internal static MethodInfo GetMethod(LambdaExpression expression) {
            MethodCallExpression memberExpression = GetMethodCallExpression(expression);
            return memberExpression.Method;
        }

        private static MethodCallExpression GetMethodCallExpression(LambdaExpression expression) {
            if(expression.Body is InvocationExpression) {
                expression = (LambdaExpression)((InvocationExpression)expression.Body).Expression;
            }
            return (MethodCallExpression)expression.Body;
        }

        public static string GetPropertyName<T>(Expression<Func<T>> expression) {
            return GetPropertyNameCore(expression);
        }
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression) {
            return GetPropertyNameCore(expression);
        }
#if !NETFX_CORE
        public static PropertyDescriptor GetProperty<T, TProperty>(Expression<Func<T, TProperty>> expression) {
            return TypeDescriptor.GetProperties(typeof(T))[GetPropertyName(expression)];
        }
#endif
        static string GetPropertyNameCore(LambdaExpression expression) {
            MemberExpression memberExpression = GetMemberExpression(expression);
            MemberExpression nextMemberExpression = memberExpression.Expression as MemberExpression;
            if(IsPropertyExpression(nextMemberExpression)) {
                throw new ArgumentException("expression");
            }
            return memberExpression.Member.Name;
        }

        static bool IsPropertyExpression(MemberExpression expression) {
            return expression != null &&
#if !NETFX_CORE
                expression.Member.MemberType == MemberTypes.Property;
#else
                expression.Member is PropertyInfo;
#endif
        }
        static MemberExpression GetMemberExpression(LambdaExpression expression) {
            if(expression == null)
                throw new ArgumentNullException("expression");
            Expression body = expression.Body;
            if(body is UnaryExpression) {
                body = ((UnaryExpression)body).Operand;
            }
            MemberExpression memberExpression = body as MemberExpression;
            if(memberExpression == null) {
                throw new ArgumentException("expression");
            }
            return memberExpression;
        }

        public static bool PropertyHasImplicitImplementation<TInterface, TPropertyType>(TInterface _interface, Expression<Func<TInterface, TPropertyType>> property, bool tryInvoke = true)
            where TInterface : class {
            if(_interface == null)
                throw new ArgumentNullException("_interface");
            string propertyName = GetArgumentPropertyStrict(property).Name;
            string getMethodName = "get_" + propertyName;
            MethodInfo getMethod = GetGetMethod(_interface, getMethodName);
            if(!getMethod.IsPublic || !string.Equals(getMethod.Name, getMethodName)) return false;
            try
            {
                if (tryInvoke)
                {
                    getMethod.Invoke(_interface, null);
                }
            }
            catch (Exception e)
            {
#if !NETFX_CORE
                if(e is TargetException) return false;
#endif
                if(e is ArgumentException) return false;
                if(e is TargetParameterCountException) return false;
                if(e is MethodAccessException) return false;
                if(e is InvalidOperationException) return false;
                throw;
            }
            return true;
        }
        static MethodInfo GetGetMethod<TInterface>(TInterface _interface, string getMethodName) {
#if !NETFX_CORE
            InterfaceMapping map = _interface.GetType().GetInterfaceMap(typeof(TInterface));
            MethodInfo getMethod = map.TargetMethods[map.InterfaceMethods
                .Select((m, i) => new { name = m.Name, index = i })
                .Where(m => string.Equals(m.name, getMethodName, StringComparison.Ordinal))
                .Select(m => m.index)
                .First()];
#else
            var expliciteGetMethodName = typeof(TInterface).FullName.Replace("+", ".") + "." + getMethodName;
            var expliciteMethod = _interface.GetType().GetRuntimeMethods().FirstOrDefault(x => string.Equals(x.Name, expliciteGetMethodName, StringComparison.Ordinal));
            var method = _interface.GetType().GetRuntimeMethods().FirstOrDefault(x => string.Equals(x.Name, getMethodName, StringComparison.Ordinal));
            MethodInfo getMethod = expliciteMethod != null ? expliciteMethod : method;
#endif
            return getMethod;
        }
    }
}