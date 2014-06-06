using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Threading;

namespace DevExpress.Xpf.Core {
    public class AgVeil {
        Canvas clickCapture;
        Dispatcher dispatcher;
        Popup ContainerPopup = new Popup();
        static int waitCounter = 0;
        static AgVeil waitVeil = null;

        public static void BeginWait() {
            BeginWait(false);
        }
        public static void BeginWait(bool grayed) {
            Dispatcher dispatcher = Application.Current.RootVisual.Dispatcher;
            if(!dispatcher.CheckAccess()) {
                throw new InvalidOperationException();
            }
            if(Interlocked.Increment(ref waitCounter) == 1) {
                Debug.Assert(waitVeil == null);
                waitVeil = new AgVeil();
                waitVeil.dispatcher = dispatcher;
                waitVeil.Show(null, grayed);
                ((FrameworkElement)waitVeil.clickCapture.Children[0]).Cursor = Cursors.Wait;
            }
        }
        public static void EndWait() {
            if(Interlocked.Decrement(ref waitCounter) == 0) {
                AgVeil veil = Interlocked.Exchange<AgVeil>(ref waitVeil, null);
                if(veil == null) {
                    return;
                }
                Action uiAction = () => {
                    if(veil != null) {
                        veil.Hide();
                    }
                };
                Dispatcher dispatcher = veil.dispatcher;
                if(!dispatcher.CheckAccess()) {
                    dispatcher.BeginInvoke(uiAction);
                }
                else {
                    uiAction();
                }
            }
        }
        public static bool IsWait { get { return waitCounter != 0; } }
        public static void DoWait(Action action) {
            BeginWait();
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(1);
            dt.Start();
            dt.Tick += (d, e) => {
                action();
                dt.Stop();
                EndWait();
            };
        }
        public void Hide() {
            ContainerPopup.IsOpen = false;
            ContainerPopup.Child = null;
            clickCapture = null;
        }
        public void Show(Action clickCallback, bool grayed) {
            if(clickCapture == null) {
                Rectangle rect = new Rectangle();
                rect.Fill = new SolidColorBrush(Colors.Black);
                rect.Opacity = grayed ? 0.1 : 0;
                SetRectSize(rect);
                rect.MouseLeftButtonDown += (d, e) => {
                    if(clickCallback != null) clickCallback();
                };
                rect.MouseRightButtonDown += (d, e) =>
                {
                    if(clickCallback != null) clickCallback();
                    else e.Handled = true;
                };
                clickCapture = new Canvas();
                clickCapture.Children.Add(rect);

                ContainerPopup.Child = clickCapture;
                ContainerPopup.IsOpen = true;
            }
        }
        protected virtual void SetRectSize(Rectangle rect) {
            rect.Width = 20000;
            rect.Height = 20000;
            rect.RenderTransform = new TranslateTransform() { X = -10000, Y = -10000 };
        }
    }
}