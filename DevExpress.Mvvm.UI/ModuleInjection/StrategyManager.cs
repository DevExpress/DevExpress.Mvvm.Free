using DevExpress.Mvvm.ModuleInjection;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public interface IStrategyManager {
        void RegisterStrategy<TTarget, TStrategy>()
            where TTarget : DependencyObject
            where TStrategy : IStrategy, new();
        void RegisterWindowStrategy<TTarget, TStrategy>()
            where TTarget : DependencyObject
            where TStrategy : IWindowStrategy, new();
        IStrategy CreateStrategy(DependencyObject target);
        IWindowStrategy CreateWindowStrategy(DependencyObject target);
    }
    public class StrategyManager : IStrategyManager {
        public static IStrategyManager Default { get { return _default ?? _instance; } set { _default = value; } }
        static IStrategyManager _default;
        static StrategyManager _instance;
        static StrategyManager() {
            _instance = new StrategyManager();
            _instance.RegisterDefaultStrategies();
        }

        readonly Dictionary<Type, Type> Strategies = new Dictionary<Type, Type>();
        readonly Dictionary<Type, Type> WindowStrategies = new Dictionary<Type, Type>();
        public void RegisterStrategy<TTarget, TStrategy>()
            where TTarget : DependencyObject
            where TStrategy : IStrategy, new() {
            RegisterStrategy<TTarget, TStrategy>(Strategies);
        }
        public void RegisterWindowStrategy<TTarget, TStrategy>()
            where TTarget : DependencyObject
            where TStrategy : IWindowStrategy, new() {
            RegisterStrategy<TTarget, TStrategy>(WindowStrategies);
        }
        public IStrategy CreateStrategy(DependencyObject target) {
            return CreateStrategy<IStrategy>(Strategies, target);
        }
        public IWindowStrategy CreateWindowStrategy(DependencyObject target) {
            return CreateStrategy<IWindowStrategy>(WindowStrategies, target);
        }

        public void RegisterDefaultStrategies() {
            RegisterStrategy<Panel, PanelStrategy<Panel, PanelWrapper>>();
            RegisterStrategy<ContentPresenter, ContentPresenterStrategy<ContentPresenter, ContentPresenterWrapper>>();
            RegisterStrategy<ContentControl, ContentPresenterStrategy<ContentControl, ContentControlWrapper>>();
            RegisterStrategy<ItemsControl, ItemsControlStrategy<ItemsControl, ItemsControlWrapper>>();
            RegisterStrategy<Selector, SelectorStrategy<Selector, SelectorWrapper>>();
            RegisterStrategy<TabControl, SelectorStrategy<TabControl, TabControlWrapper>>();
            RegisterWindowStrategy<Window, WindowStrategy<Window, WindowWrapper>>();
        }

        static void RegisterStrategy<TTarget, TStrategy>(Dictionary<Type, Type> strategies) where TStrategy : new() {
            Type tTarget = typeof(TTarget);
            Type tStrategy = typeof(TStrategy);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(tTarget.TypeHandle);
            if(strategies.ContainsKey(tTarget))
                strategies[tTarget] = tStrategy;
            else strategies.Add(tTarget, tStrategy);
        }
        static TStrategy CreateStrategy<TStrategy>(Dictionary<Type, Type> strategies, DependencyObject target) {
            if(target == null) return default(TStrategy);
            Type tTarget = target.GetType();
            Type tStrategy = null;
            do {
                foreach(Type currentTTarget in strategies.Keys) {
                    if(currentTTarget == tTarget) {
                        tStrategy = strategies[currentTTarget];
                        break;
                    }
                }
                tTarget = tTarget.BaseType;
            } while(tTarget != null && tStrategy == null);
            if(tStrategy == null) ModuleInjectionException.NoStrategy(target.GetType());
            return (TStrategy)Activator.CreateInstance(tStrategy, null);
        }
    }
}