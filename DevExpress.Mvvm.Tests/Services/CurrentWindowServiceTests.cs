using NUnit.Framework;
using System.Windows.Controls;
using System.Threading.Tasks;
using System;
using System.Windows.Media;
using System.Threading;
using System.Windows.Data;
using System.Windows;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class CurrentWindowServiceTests : BaseWpfFixture {
        [Test(Description = "T486009"), Asynchronous]
        public void ActualWindow_HiddenWindow() {
            CurrentWindowService service = new CurrentWindowService();
            ICurrentWindowService iService = service;
            Interactivity.Interaction.GetBehaviors(RealWindow).Add(service);
            try {
                EnqueueCallback(() => {
                    Assert.AreSame(RealWindow, service.ActualWindow);
                });
                EnqueueTestComplete();
            } finally {
                Interactivity.Interaction.GetBehaviors(RealWindow).Remove(service);
            }
        }
        [Test, Asynchronous]
        public void ActualWindow() {
            CurrentWindowService service = new CurrentWindowService();
            ICurrentWindowService iService = service;
            Grid grid = new Grid();
            Interactivity.Interaction.GetBehaviors(grid).Add(service);
            service.Window = new Window();
            service.Window = null;
            RealWindow.Content = grid;
            EnqueueShowRealWindow();
            EnqueueCallback(() => {
                Assert.AreSame(RealWindow, service.ActualWindow);
                service.Window = new Window();
                Assert.AreNotSame(RealWindow, service.ActualWindow);
                service.Window = null;
                Assert.AreSame(RealWindow, service.ActualWindow);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void ShowHide() {
            CurrentWindowService service = new CurrentWindowService();
            ICurrentWindowService iService = service;
            Grid grid = new Grid();
            Interactivity.Interaction.GetBehaviors(grid).Add(service);
            RealWindow.Content = grid;
            EnqueueShowRealWindow();
            EnqueueCallback(() => {
                iService.Hide();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.IsFalse(RealWindow.IsVisible);
                iService.Show();
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.IsTrue(RealWindow.IsVisible);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void SetWindowState() {
            CurrentWindowService service = new CurrentWindowService();
            ICurrentWindowService iService = service;
            Grid grid = new Grid();
            Interactivity.Interaction.GetBehaviors(grid).Add(service);
            RealWindow.Content = grid;
            EnqueueShowRealWindow();
            EnqueueCallback(() => {
                iService.SetWindowState(WindowState.Maximized);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(WindowState.Maximized, RealWindow.WindowState);
                iService.SetWindowState(WindowState.Minimized);
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(WindowState.Minimized, RealWindow.WindowState);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void Activate() {
            CurrentWindowService service = new CurrentWindowService();
            ICurrentWindowService iService = service;
            Grid grid = new Grid();
            Interactivity.Interaction.GetBehaviors(grid).Add(service);
            RealWindow.Content = grid;
            RealWindow.ShowActivated = false;
            EnqueueShowRealWindow();
            EnqueueCallback(() => {
                Assert.IsFalse(RealWindow.IsActive);
                iService.Activate();
                Assert.IsTrue(RealWindow.IsActive);
            });
            EnqueueTestComplete();
        }
    }
}