using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace DevExpress {
    internal static class DispatcherHelper {
        static Dispatcher CurrentDispatcherReference;
        static DispatcherHelper() {
            IncreasePriorityContextIdleMessages();
        }
        static void IncreasePriorityContextIdleMessages() {
            CurrentDispatcherReference = Dispatcher.CurrentDispatcher;
            Dispatcher.CurrentDispatcher.Hooks.OperationPosted += (d, e) => {
                if(e.Operation.Priority == DispatcherPriority.ContextIdle)
                    e.Operation.Priority = DispatcherPriority.Background;
            };
        }
        static object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        public static void ForceIncreasePriorityContextIdleMessages() {
        }
        public static void UpdateLayoutAndDoEvents(UIElement element) { UpdateLayoutAndDoEvents(element, DispatcherPriority.Background); }
        public static void UpdateLayoutAndDoEvents(UIElement element, DispatcherPriority priority) {
            element.UpdateLayout();
            DoEvents(priority);
        }
        public static void DoEvents() {
            DoEvents(DispatcherPriority.Background);
        }
        public static void DoEvents(DispatcherPriority priority) {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                priority,
                new DispatcherOperationCallback(ExitFrame),
                frame);
            Dispatcher.PushFrame(frame);
        }
    }
}