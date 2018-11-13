using System;
using System.Linq;
using System.Reflection;

namespace DevExpress.Mvvm.Native {
    public static class DelegateCommandFactory {
        internal static MethodInfo GetGenericMethodWithoutResult(Type parameterType, bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("Create", withUseCommandManagerParameter ? new Type[] { typeof(Action<>), typeof(Func<,>), typeof(bool) } : new Type[] { typeof(Action<>), typeof(Func<,>) });
            return method.MakeGenericMethod(parameterType);
        }
        internal static MethodInfo GetGenericMethodWithResult(Type parameterType1, Type parameterType2, bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("CreateFromFunction", withUseCommandManagerParameter ? new Type[] { typeof(Func<,>), typeof(Func<,>), typeof(bool) } : new Type[] { typeof(Func<,>), typeof(Func<,>) });
            return method.MakeGenericMethod(parameterType1, parameterType2);
        }
        internal static MethodInfo GetSimpleMethodWithoutResult(bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("Create", withUseCommandManagerParameter ? new Type[] { typeof(Action), typeof(Func<bool>), typeof(bool) } : new Type[] { typeof(Action), typeof(Func<bool>) });
            return method;
        }
        internal static MethodInfo GetSimpleMethodWithResult(Type parameterType, bool withUseCommandManagerParameter) {
            var method = GetMethodByParameter("CreateFromFunction", withUseCommandManagerParameter ? new Type[] { typeof(Func<>), typeof(Func<bool>), typeof(bool) } : new Type[] { typeof(Func<>), typeof(Func<bool>) });
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

        public static DelegateCommand<T> CreateFromFunction<T, TResult>(Func<T, TResult> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager) {
            return new DelegateCommand<T>(x => executeMethod(x), canExecuteMethod, useCommandManager);
        }
        public static DelegateCommand<T> CreateFromFunction<T, TResult>(Func<T, TResult> executeMethod, Func<T, bool> canExecuteMethod) {
            return new DelegateCommand<T>(x => executeMethod(x), canExecuteMethod);
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod, bool useCommandManager) {
            return new DelegateCommand<T>(executeMethod, useCommandManager);
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod) {
            return new DelegateCommand<T>(executeMethod);
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager) {
            return new DelegateCommand<T>(executeMethod, canExecuteMethod, useCommandManager);
        }
        public static DelegateCommand<T> Create<T>(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
            return new DelegateCommand<T>(executeMethod, canExecuteMethod);
        }

        public static DelegateCommand CreateFromFunction<TResult>(Func<TResult> executeMethod, Func<bool> canExecuteMethod, bool useCommandManager) {
            return new DelegateCommand(() => executeMethod(), canExecuteMethod, useCommandManager);
        }
        public static DelegateCommand CreateFromFunction<TResult>(Func<TResult> executeMethod, Func<bool> canExecuteMethod) {
            return new DelegateCommand(() => executeMethod(), canExecuteMethod);
        }
        public static DelegateCommand Create(Action executeMethod, bool useCommandManager) {
            return new DelegateCommand(executeMethod, useCommandManager);
        }
        public static DelegateCommand Create(Action executeMethod) {
            return new DelegateCommand(executeMethod);
        }
        public static DelegateCommand Create(Action executeMethod, Func<bool> canExecuteMethod, bool useCommandManager) {
            return new DelegateCommand(executeMethod, canExecuteMethod, useCommandManager);
        }
        public static DelegateCommand Create(Action executeMethod, Func<bool> canExecuteMethod) {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }
    }
}