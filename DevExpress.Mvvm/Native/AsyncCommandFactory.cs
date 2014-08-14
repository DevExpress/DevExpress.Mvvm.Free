using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.Native {
    public static class AsyncCommandFactory {
        internal static MethodInfo GetGenericMethodWithResult(Type parameterType1, Type parameterType2, bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("CreateFromFunction", withUseCommandManagerParameter ? new Type[] { typeof(Func<,>), typeof(Func<,>), typeof(bool) } : new Type[] { typeof(Func<,>), typeof(Func<,>) });
            return method.MakeGenericMethod(parameterType1, parameterType2);
        }
        internal static MethodInfo GetSimpleMethodWithResult(Type parameterType, bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("CreateFromFunction", withUseCommandManagerParameter ? new Type[] { typeof(Func<>), typeof(Func<bool>), typeof(bool) } : new Type[] { typeof(Func<>), typeof(Func<bool>) });
            return method.MakeGenericMethod(parameterType);
        }
        static MethodInfo GetMethodByParameter(string methodName, Type[] parameterTypes) {
            Type asyncCommandFactoryType = typeof(AsyncCommandFactory);
            MethodInfo[] methodInfos = asyncCommandFactoryType.GetMethods();
            var methods = methodInfos.Where(m => m.Name == methodName);
            foreach(MethodInfo methodInfo in methods) {
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if(parameterInfos.Length != parameterTypes.Length)
                    continue;
                bool isThisMatched = true;
                for(int i = 0; i < parameterInfos.Length; i++) {
                    if(parameterInfos[i].ParameterType.Name != parameterTypes[i].Name) {
                        isThisMatched = false;
                        break;
                    }
                }
                if(isThisMatched)
                    return methodInfo;
            }
            return null;
        }

        public static AsyncCommand<T> CreateFromFunction<T, TResult>(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager) {
#if !SILVERLIGHT
            return new AsyncCommand<T>(x => executeMethod(x), canExecuteMethod, useCommandManager);
#else
            return new AsyncCommand<T>(x => executeMethod(x), canExecuteMethod);
#endif
        }
        public static AsyncCommand<T> CreateFromFunction<T, TResult>(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) {
            return new AsyncCommand<T>(x => executeMethod(x), canExecuteMethod);
        }
        public static AsyncCommand CreateFromFunction<TResult>(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool useCommandManager) {
#if !SILVERLIGHT
            return new AsyncCommand(() => executeMethod(), canExecuteMethod, useCommandManager);
#else
            return new AsyncCommand(() => executeMethod(), canExecuteMethod);
#endif
        }
        public static AsyncCommand CreateFromFunction<TResult>(Func<Task> executeMethod, Func<bool> canExecuteMethod) {
            return new AsyncCommand(() => executeMethod(), canExecuteMethod);
        }
    }
}