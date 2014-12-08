#if !NETFX_CORE
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace DevExpress.Mvvm.UI.Tests {
    public partial class EventToCommandTestView : UserControl {
        public EventToCommandTestView() {
            InitializeComponent();
        }
    }
}