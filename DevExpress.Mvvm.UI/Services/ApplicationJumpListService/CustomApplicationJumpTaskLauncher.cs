using System;
using System.Diagnostics;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {
    public static class CustomApplicationJumpTaskLauncher {
        public static void Run(string[] args) {
            JumpActionsManagerClient client = new JumpActionsManagerClient();
            client.Run(args, s => Process.Start(s));
        }
    }
}