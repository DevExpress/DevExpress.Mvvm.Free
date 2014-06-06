using System;
using System.Collections.Generic;

namespace DevExpress {
    public class WindowManager {
        [ThreadStatic]
        private static WindowManager defaultInstance = null;
        private List<WeakReference> windows = new List<WeakReference>();
        public void HideAllWindows() {
            foreach(WeakReference wr in this.windows) {
                DXWindowBase window = (DXWindowBase)wr.Target;
                if(window != null) {
                    window.Hide();
                }
            }
        }
        public void Register(DXWindowBase window) {
            this.windows.Add(new WeakReference(window));
        }

        public void SetTheme() {
        }

        public void SetWindowActive(DXWindowBase window) {
            foreach(WeakReference reference in this.windows) {
                if(reference.IsAlive) {
                    DXWindowBase currentWindow = reference.Target as DXWindowBase;
                    if(currentWindow == window) {
                        currentWindow.SetActive(true);
                        this.ActiveWindow = reference;
                    } else {
                        currentWindow.SetActive(false);
                    }
                }
            }
        }
        public WeakReference ActiveWindow {
            get;
            private set;
        }

        public static WindowManager Default {
            get {
                if(defaultInstance == null) {
                    defaultInstance = new WindowManager();
                }
                return defaultInstance;
            }
        }
    }
}