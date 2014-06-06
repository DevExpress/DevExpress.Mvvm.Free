using System;
using System.Linq;
using System.Reflection;

namespace DevExpress.Mvvm.Native {
    public static class DelegateCommandFactory {
        internal static MethodInfo GetGenericMethodWithoutResult(Type parameterType) {
            var method = GetMethodByParameter("Create", new Type[] { typeof(Action<>), typeof(Func<,>), typeof(bool) });
            return method.MakeGenericMethod(parameterType);
        }
        internal static MethodInfo GetGenericMethodWithResult(Type parameterType1, Type parameterType2) {
            var method = GetMethodByParameter("CreateFromFunction", new Type[] { typeof(Func<,>), typeof(Func<,>), typeof(bool) });
            return method.MakeGenericMethod(parameterType1, parameterType2);
        }
        internal static MethodInfo GetSimpleMethodWithoutResult() {
            var method = GetMethodByParameter("Create", new Type[] { typeof(Action), typeof(Func<bool>), typeof(bool) });
            return method;
        }
        internal static MethodInfo GetSimpleMethodWithResult(Type parameterType) {
            var method = GetMethodByParameter("CreateFromFunction", new Type[] { typeof(Func<>), typeof(Func<bool>), typeof(bool) });
            return method.MakeGenericMethod(parameterType);
        }
        static MethodInfo GetMethodByParameter(string methodName, Type[] parameterTypes) {
            Type delegateCommandFactoryType = typeof(DelegateCommandFactory);
            MethodInfo[] methodInfos = delegateCommandFactoryType.GetMethods();
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

        public static DelegateCommand<T> CreateFromFunction<T, TResult>(Func<T, TResult> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand<T>(x => executeMethod(x), canExecuteMethod, useCommandManager);
#else
            return new DelegateCommand<T>(x => executeMethod(x), canExecuteMethod);
#endif
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand<T>(executeMethod, useCommandManager);
#else
            return new DelegateCommand<T>(executeMethod);
#endif
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand<T>(executeMethod, canExecuteMethod, useCommandManager);
#else
            return new DelegateCommand<T>(executeMethod, canExecuteMethod);
#endif
        }

        public static DelegateCommand CreateFromFunction<TResult>(Func<TResult> executeMethod, Func<bool> canExecuteMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand(() => executeMethod(), canExecuteMethod, useCommandManager);
#else
            return new DelegateCommand(() => executeMethod(), canExecuteMethod);
#endif
        }
        public static DelegateCommand Create(Action executeMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand(executeMethod, useCommandManager);
#else
            return new DelegateCommand(executeMethod);
#endif
        }
        public static DelegateCommand Create(Action executeMethod, Func<bool> canExecuteMethod, bool useCommandManager = true) {
#if !SILVERLIGHT
            return new DelegateCommand(executeMethod, canExecuteMethod, useCommandManager);
#else
            return new DelegateCommand(executeMethod, canExecuteMethod);
#endif
        }
    }
}