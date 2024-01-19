using System;
using System.Diagnostics;
using System.Windows.Forms;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI.ApplicationJumpTaskLauncher {
    class Program {
#pragma warning disable DX0015
        static void Main(string[] args) {
            try {
                JumpActionsManagerClient client = new JumpActionsManagerClient();
                client.Run(args, s => Process.Start(s));
            } catch(Exception e) {
                new ThreadExceptionDialog(e).ShowDialog();
            }
        }
#pragma warning restore DX0015
    }
}