using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevExpress {
    public static class UITestHelper {
        public static bool IsElementVisible(FrameworkElement element) {
            return element.Visibility == Visibility.Visible;
        }
        public static bool IsElementFocused(UIElement element) {
#if SILVERLIGHT
            return element != null && element == FocusManager.GetFocusedElement();
#else
            return element.IsFocused;
#endif
        }
        public static void ClickButton(ButtonBase button) {
            ButtonBaseAutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(button) as ButtonBaseAutomationPeer;
            IInvokeProvider invoker = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invoker.Invoke();
            DispatcherHelper.UpdateLayoutAndDoEvents(button);
        }
    }
}