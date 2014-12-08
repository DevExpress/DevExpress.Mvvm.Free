#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
using DevExpress.Mvvm.Tests.TestUtils;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using DevExpress.TestUtils;
#else
using NUnit.Framework;
#endif
#if !FREE && !NETFX_CORE
using DevExpress.Xpf.Core.Tests;
#endif
using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.POCO;
using System.Windows.Threading;
using System.Windows.Data;
#else
#endif

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class EventToCommandTests : BaseWpfFixture {
        [Test, Asynchronous]
#if !NETFX_CORE
        public void EventToCommandInWindow() {
#else
        public async Task EventToCommandInWindow() {
#endif
            var userControl = new EventToCommandTestView();
            Window.Content = userControl;
            var viewModel = (EventToCommandTestViewModel)userControl.DataContext;
            Assert.AreEqual(0, viewModel.LoadedCount);
            Assert.AreEqual(0, viewModel.SelectionChangedCount);
#if NETFX_CORE
            await
#endif
            EnqueueTestWindowMainCallback(() => {
                foreach(EventToCommand trigger in Interaction.GetBehaviors(userControl)) {
                    Assert.AreEqual(1, trigger.RaiseSourceChangedCount);
                }


                Assert.AreEqual(1, viewModel.LoadedCount);
                Assert.AreEqual("test", viewModel.LoadedParameter);
                Assert.AreEqual(0, viewModel.DummyCommand.InvokeCount);

                Assert.AreEqual(1, viewModel.ButtonLoadedCount);

                Assert.AreEqual(0, viewModel.SelectionChangedCount);
                userControl.listBox.SelectedIndex = 1;
                Assert.AreEqual(1, viewModel.SelectionChangedCount);
                Assert.AreEqual("item2", viewModel.SelectionChangedParameter.AddedItems[0].With(x => x as ListBoxItem).Content);

                Assert.AreEqual(0, viewModel.SelectionChangedCount2);
                userControl.listBox2.SelectedIndex = 1;
                Assert.AreEqual(1, viewModel.SelectionChangedCount2);
                Assert.AreEqual("foo", viewModel.SelectionChangedParameter2);

                Assert.AreEqual(0, viewModel.SelectionChangedCount3);
                userControl.listBox3.SelectedIndex = 1;
                Assert.AreEqual(1, viewModel.SelectionChangedCount3);
                Assert.AreEqual("item2", viewModel.SelectionChangedParameter3.With(x => x as ListBoxItem).Content);

                Assert.IsFalse(userControl.listBox4.IsEnabled);
                viewModel.SelectionChangedCommandParameter4 = true;
                Assert.IsTrue(userControl.listBox4.IsEnabled);
                viewModel.SelectionChangedCommandParameter4 = false;
                Assert.IsFalse(userControl.listBox4.IsEnabled);
                var eventToCommand = (EventToCommand)Interaction.GetBehaviors(userControl.listBox4)[0];
                eventToCommand.Command = viewModel.SelectionChangedCommand2;
                Assert.IsTrue(userControl.listBox4.IsEnabled);

                eventToCommand.AllowChangingEventOwnerIsEnabled = false;
                eventToCommand.Command = viewModel.SelectionChangedCommand4;
                Assert.IsTrue(userControl.listBox4.IsEnabled);
                eventToCommand.AllowChangingEventOwnerIsEnabled = true;
                Assert.IsFalse(userControl.listBox4.IsEnabled);
                Assert.IsFalse(userControl.listBox5.IsEnabled);
                viewModel.SelectionChangedCommandParameter5 = true;
                Assert.IsTrue(userControl.listBox5.IsEnabled);
                viewModel.SelectionChangedCommandParameter5 = false;
                Assert.IsFalse(userControl.listBox5.IsEnabled);
                eventToCommand = (EventToCommand)Interaction.GetBehaviors(userControl).First(x => ((EventToCommand)x).SourceName == "listBox5");
                eventToCommand.Command = viewModel.SelectionChangedCommand2;
                Assert.IsTrue(userControl.listBox5.IsEnabled);

                eventToCommand.AllowChangingEventOwnerIsEnabled = false;
                eventToCommand.Command = viewModel.SelectionChangedCommand5;
                Assert.IsTrue(userControl.listBox5.IsEnabled);
                eventToCommand.AllowChangingEventOwnerIsEnabled = true;
                Assert.IsFalse(userControl.listBox5.IsEnabled);
            });
            EnqueueTestComplete();
        }
#if !NETFX_CORE
        [Test, Asynchronous]
        public void SourceChangedFireCount1() {
            var panel = new StackPanel();
            panel.Children.Add(new Button() { Name = "button1" });
            panel.Children.Add(new Button() { Name = "button2" });
            int gotFocusCount = 0;
            var eventToCommand = new EventToCommand() { SourceObject = panel.Children[0], EventName = "GotFocus", Command = DelegateCommandFactory.Create(() => gotFocusCount++, false) };
            Interaction.GetBehaviors(panel).Add(eventToCommand);
            Window.Content = panel;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(1, eventToCommand.RaiseSourceChangedCount);
                eventToCommand.SourceName = "button2";
                Assert.AreEqual(1, eventToCommand.RaiseSourceChangedCount);

                Assert.AreEqual(0, gotFocusCount);
                ((Button)panel.Children[1]).Focus();
            });
            EnqueueConditional(() => gotFocusCount == 0);
            EnqueueCallback(() => {
                ((Button)panel.Children[0]).Focus();
            });
            EnqueueConditional(() => gotFocusCount == 1);
            EnqueueTestComplete();
        }
#endif
        [Test, Asynchronous]
#if !NETFX_CORE
        public void SourceChangedFireCount2() {
#else
        public async Task SourceChangedFireCount2() {
#endif
            var panel = new StackPanel();
            panel.Children.Add(new Button() { Name = "button2" });
            int gotFocusCount = 0;
            var eventToCommand = new EventToCommand() { EventName = "GotFocus", Command = new DelegateCommand(() => gotFocusCount++) };
            Interaction.GetBehaviors(panel).Add(eventToCommand);
            Window.Content = panel;
#if NETFX_CORE
            await
#endif
            EnqueueTestWindowMainCallback(() => {
                Assert.AreEqual(1, eventToCommand.RaiseSourceChangedCount);
                eventToCommand.SourceName = "button2";

            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, eventToCommand.RaiseSourceChangedCount);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void B236199_DataContextChangedSubscription() {
#else
        public async Task B236199_DataContextChangedSubscription() {
#endif
            var button = new Button();
            int dataContextChangedCount = 0;
            int dataContextChangedCount2 = 0;
            button.DataContextChanged += (d, e) => dataContextChangedCount2++;
            var eventToCommand = new EventToCommand() { EventName = "DataContextChanged", Command = new DelegateCommand(() => dataContextChangedCount++) };
            Interaction.GetBehaviors(button).Add(eventToCommand);
            Window.Content = button;
#if NETFX_CORE
            await
#endif
            EnqueueTestWindowMainCallback(() => {
                Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
                button.DataContext = "1";
                Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
            });
            EnqueueTestComplete();
        }

        [Test]
        public void NameScopeAccessProviderSourceTest() {
            var window = new ContentControl();
            var eventToCommand = new EventToCommand();
            var testViewModel = new TestViewModel();
            window.Content = testViewModel;
            int execCount = 0;
            eventToCommand.Command = new DelegateCommand(() => execCount++);
            eventToCommand.SourceObject = testViewModel;
            eventToCommand.EventName = "TestEvent";
            Interaction.GetBehaviors(window).Add(eventToCommand);
            testViewModel.RaiseTestEvent();
            Assert.AreEqual(1, execCount);
        }
        public class TestViewModel {
            public event Action<object, object> TestEvent = (o1, o2) => { };
            public void RaiseTestEvent() {
                TestEvent(null, null);
            }
        }

        [Test, Asynchronous]
#if !NETFX_CORE
        public void B250383() {
#else
        public async Task B250383() {
#endif
            var control = new Button();
            control.IsEnabled = false;
            int loaded = 0;
            var eventToCommand = new EventToCommand() { EventName = "Loaded", Command = new DelegateCommand(() => loaded++) };
            Interaction.GetBehaviors(control).Add(eventToCommand);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueTestWindowMainCallback(() => {
                Assert.AreEqual(1, loaded);
                eventToCommand.SourceName = "button2";
            });

            EnqueueCallback(() => {
                control = new Button();
                control.IsEnabled = false;
                eventToCommand = new EventToCommand() { EventName = "Loaded", ProcessEventsFromDisabledEventOwner = false, Command = new DelegateCommand(() => loaded++) };
                Interaction.GetBehaviors(control).Add(eventToCommand);
                loaded = 0;
                Window.Content = control;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(0, loaded);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q539009_1() {
#else
        public async Task Q539009_1() {
#endif
            var control = new Grid();
            int counter2 = 0;
            int counter1 = 0;
            control.SizeChanged += (d, e) => counter2++;
            var eventToCommand = new EventToCommand() {
                EventName = "SizeChanged",
                Command = new DelegateCommand(() => counter1++),
            };
            Interaction.GetBehaviors(control).Add(eventToCommand);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(counter2, counter1);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q539009_2() {
#else
        public async Task Q539009_2() {
#endif
            var control = new Grid() { Name = "control" };
            int counter2 = 0;
            int counter1 = 0;
            control.SizeChanged += (d, e) => counter2++;
            var eventToCommand = new EventToCommand() {
                SourceName = "control",
                EventName = "SizeChanged",
                Command = new DelegateCommand(() => counter1++),
            };
            Interaction.GetBehaviors(control).Add(eventToCommand);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(counter2, counter1);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q539009_3() {
#else
        public async Task Q539009_3() {
#endif
            var control = new Grid() { Name = "control" };
            int counter2 = 0;
            int counter1 = 0;
            control.SizeChanged += (d, e) => counter2++;
            var eventToCommand = new EventToCommand() {
                SourceObject = control,
                EventName = "SizeChanged",
                Command = new DelegateCommand(() => counter1++),
            };
            Interaction.GetBehaviors(control).Add(eventToCommand);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(counter2, counter1);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q554072_11() {
#else
        public async Task Q554072_11() {
            await
#endif
            Q554072_1Core("Loaded");
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q554072_12() {
#else
        public async Task Q554072_12() {
            await
#endif
            Q554072_1Core("SizeChanged");
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q554072_21() {
#else
        public async Task Q554072_21() {
            await
#endif
            Q554072_2Core("Loaded");
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q554072_22() {
#else
        public async Task Q554072_22() {
            await
#endif
            Q554072_2Core("SizeChanged");
        }
#if !NETFX_CORE
        void Q554072_1Core(string eventName) {
#else
        async Task Q554072_1Core(string eventName) {
#endif
            var control = new Grid();
            var bt = new Button() { Name = "View" };
            control.Children.Add(bt);
            int counter1 = 0;
            int counter2 = 0;
            int counter3 = 0;
            control.Loaded += (d, e) => counter1++;
            var eventToCommand1 = new EventToCommand() {
                PassEventArgsToCommand = true,
                EventName = eventName,
                Command = new DelegateCommand(() => counter2++),
                SourceName = "View",
            };
            var eventToCommand2 = new EventToCommand() {
                PassEventArgsToCommand = true,
                EventName = eventName,
                Command = new DelegateCommand(() => counter3++),
                SourceName = "View",
            };
            Interaction.GetBehaviors(control).Add(eventToCommand1);
            Interaction.GetBehaviors(bt).Add(eventToCommand2);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(counter2, counter1);
                Assert.AreEqual(counter3, counter1);
            });
            EnqueueTestComplete();
        }
#if !NETFX_CORE
        void Q554072_2Core(string eventName) {
#else
        async Task Q554072_2Core(string eventName) {
#endif
            var control = new Grid();
            var bt = new Button() { Name = "View" };
            control.Children.Add(bt);
            int counter1 = 0;
            int counter2 = 0;
            control.Loaded += (d, e) => counter1++;
            var eventToCommand1 = new EventToCommand() {
                PassEventArgsToCommand = true,
                EventName = eventName,
                Command = new DelegateCommand(() => counter2++),
            };
            BindingOperations.SetBinding(eventToCommand1, EventToCommand.SourceObjectProperty, new Binding() { ElementName = "View" });
            Interaction.GetBehaviors(control).Add(eventToCommand1);
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                var evv = eventToCommand1.SourceObject;
                Assert.AreEqual(counter2, counter1);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
#if !NETFX_CORE
        public void Q554072_3() {
#else
        public async Task Q554072_3() {
#endif
            var control = new EventToCommandTestView();
            Window.Content = control;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                EventToCommandTestViewModel vm = (EventToCommandTestViewModel)control.DataContext;
#if !NETFX_CORE
                Assert.AreEqual(2, vm.Q554072CommandCount);
#else
                Assert.IsTrue(vm.Q554072CommandCount > 1);
#endif
            });
            EnqueueTestComplete();
        }
#if !SILVERLIGHT && !NETFX_CORE
        [Test]
        public void SetEvent_CheckEventNameIsReset_SetEventName_CheckEventIsReset() {
            EventToCommand eventToCommand = new EventToCommand();
            Assert.IsNotNull(eventToCommand.EventName);
            eventToCommand.Event = Validation.ErrorEvent;
            Assert.IsNull(eventToCommand.EventName);
            eventToCommand.EventName = "Unloaded";
            Assert.IsNull(eventToCommand.Event);
        }
        [Test]
        public void SetEvent_RaiseEvent_CheckCommandExecuted() {
            bool commandExecuted = false;
            EventToCommand eventToCommand = new EventToCommand() {
                Command = new DelegateCommand(() => {
                    commandExecuted = true;
                })
            };
            Button button = new Button();
            eventToCommand.Event = Button.ClickEvent;
            eventToCommand.Attach(button);
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
            EnqueueWindowUpdateLayout();
            Assert.IsTrue(commandExecuted);
        }
#endif
#if !NETFX_CORE
        [Test, Asynchronous]
        public void EventSenderTest1() {
            EventSenderTestCore(true, true);
        }
        [Test, Asynchronous]
        public void EventSenderTest2() {
            EventSenderTestCore(false, true);
        }
        [Test, Asynchronous]
        public void EventSenderTest3() {
            EventSenderTestCore(true, false);
        }
        [Test, Asynchronous]
        public void EventSenderTest4() {
            EventSenderTestCore(false, false);
        }
        public void EventSenderTestCore(bool addControlFirst, bool linearLayout) {
            Grid rootRoot = new Grid();
            Grid root = new Grid();
            Button bt = null;
            if(addControlFirst) {
                if(linearLayout) {
                    bt = new Button() { Name = "bt" };
                    root.Children.Add(bt);
                } else {
                    bt = new Button() { Name = "bt" };
                    rootRoot.Children.Add(bt);
                    rootRoot.Children.Add(root);
                }
            }


            EventToCommandTestClass eventToCommand1 = new EventToCommandTestClass() { EventName = "SizeChanged" };
            EventToCommandTestClass eventToCommand2 = new EventToCommandTestClass() { EventName = "SizeChanged" };
            BindingOperations.SetBinding(eventToCommand2, EventToCommand.SourceObjectProperty, new Binding() { ElementName = "bt" });
            EventToCommandTestClass eventToCommand3 = new EventToCommandTestClass() { EventName = "SizeChanged", SourceName = "bt" };

            EventToCommandTestClass eventToCommand4 = new EventToCommandTestClass() { EventName = "Loaded" };
            EventToCommandTestClass eventToCommand5 = new EventToCommandTestClass() { EventName = "Loaded" };
            BindingOperations.SetBinding(eventToCommand5, EventToCommand.SourceObjectProperty, new Binding() { ElementName = "bt" });
            EventToCommandTestClass eventToCommand6 = new EventToCommandTestClass() { EventName = "Loaded", SourceName = "bt" };

            Interaction.GetBehaviors(root).Add(eventToCommand1);
            Interaction.GetBehaviors(root).Add(eventToCommand2);
            Interaction.GetBehaviors(root).Add(eventToCommand3);
            Interaction.GetBehaviors(root).Add(eventToCommand4);
            Interaction.GetBehaviors(root).Add(eventToCommand5);
            Interaction.GetBehaviors(root).Add(eventToCommand6);

            if(!addControlFirst) {
                if(linearLayout) {
                    bt = new Button() { Name = "bt" };
                    root.Children.Add(bt);
                } else {
                    bt = new Button() { Name = "bt" };
                    rootRoot.Children.Add(bt);
                    rootRoot.Children.Add(root);
                }
            }
            if(linearLayout)
                Window.Content = root;
            else
                Window.Content = rootRoot;
            EnqueueShowWindow();
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, eventToCommand1.EventCount);
                Assert.AreEqual(root, eventToCommand1.EventSender);
                Assert.AreEqual(EventToCommandType.AssociatedObject, eventToCommand1.Type);

                Assert.AreEqual(1, eventToCommand2.EventCount);
                Assert.AreEqual(bt, eventToCommand2.EventSender);
                Assert.AreEqual(EventToCommandType.SourceObject, eventToCommand2.Type);

                Assert.AreEqual(1, eventToCommand3.EventCount);
                Assert.AreEqual(bt, eventToCommand3.EventSender);
                Assert.AreEqual(EventToCommandType.SourceName, eventToCommand3.Type);

                Assert.AreEqual(1, eventToCommand4.EventCount);
                Assert.AreEqual(root, eventToCommand4.EventSender);
                Assert.AreEqual(EventToCommandType.AssociatedObject, eventToCommand4.Type);

                Assert.AreEqual(1, eventToCommand5.EventCount);
                Assert.AreEqual(bt, eventToCommand5.EventSender);
                Assert.AreEqual(EventToCommandType.SourceObject, eventToCommand5.Type);

                Assert.AreEqual(1, eventToCommand6.EventCount);
                Assert.AreEqual(bt, eventToCommand6.EventSender);
                Assert.AreEqual(EventToCommandType.SourceName, eventToCommand6.Type);
            });
            EnqueueTestComplete();
        }
#else
        [Test, Asynchronous]
        public async Task EventSenderTest1() {
            await EventSenderTestCore(true, true);
        }
        [Test, Asynchronous]
        public async Task EventSenderTest2() {
            await EventSenderTestCore(false, true);
        }
        [Test, Asynchronous]
        public async Task EventSenderTest3() {
            await EventSenderTestCore(true, false);
        }
        [Test, Asynchronous]
        public async Task EventSenderTest4() {
            await EventSenderTestCore(false, false);
        }
        class TestButton : Button {
            public TestButton() {
                Loaded += TestButton_Loaded;
            }

            async void TestButton_Loaded(object sender, RoutedEventArgs e) {
                await Task.Delay(35);
                Width *= 2;
            }
        }
        public async Task EventSenderTestCore(bool addControlFirst, bool linearLayout) {
            Grid rootRoot = new Grid();
            Grid root = new Grid();
            Button bt = null;
            if(addControlFirst) {
                if(linearLayout) {
                    bt = new TestButton() { Name = "bt" };
                    root.Children.Add(bt);
                } else {
                    bt = new TestButton() { Name = "bt" };
                    rootRoot.Children.Add(bt);
                    rootRoot.Children.Add(root);
                }
            }

            EventToCommandTestClass eventToCommand1 = new EventToCommandTestClass() { EventName = "SizeChanged" };
            EventToCommandTestClass eventToCommand2 = new EventToCommandTestClass() { EventName = "SizeChanged" };
            BindingOperations.SetBinding(eventToCommand2, EventToCommand.SourceObjectProperty, new Binding() { ElementName = "bt" });
            EventToCommandTestClass eventToCommand3 = new EventToCommandTestClass() { EventName = "SizeChanged", SourceName = "bt" };

            EventToCommandTestClass eventToCommand4 = new EventToCommandTestClass() { EventName = "Loaded" };
            EventToCommandTestClass eventToCommand5 = new EventToCommandTestClass() { EventName = "Loaded" };
            BindingOperations.SetBinding(eventToCommand5, EventToCommand.SourceObjectProperty, new Binding() { ElementName = "bt" });
            EventToCommandTestClass eventToCommand6 = new EventToCommandTestClass() { EventName = "Loaded", SourceName = "bt" };

            Interaction.GetBehaviors(root).Add(eventToCommand1);
            Interaction.GetBehaviors(root).Add(eventToCommand2);
            Interaction.GetBehaviors(root).Add(eventToCommand3);
            Interaction.GetBehaviors(root).Add(eventToCommand4);
            Interaction.GetBehaviors(root).Add(eventToCommand5);
            Interaction.GetBehaviors(root).Add(eventToCommand6);

            if(!addControlFirst) {
                if(linearLayout) {
                    bt = new TestButton() { Name = "bt" };
                    root.Children.Add(bt);
                } else {
                    bt = new TestButton() { Name = "bt" };
                    rootRoot.Children.Add(bt);
                    rootRoot.Children.Add(root);
                }
            }
            bt.Loaded += bt_Loaded;
            bt.SizeChanged += bt_SizeChanged;
            if(linearLayout)
                Window.Content = root;
            else
                Window.Content = rootRoot;
            await EnqueueShowWindow();
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, eventToCommand1.EventCount);
                Assert.AreEqual(root, eventToCommand1.EventSender);
                Assert.AreEqual(EventToCommandType.AssociatedObject, eventToCommand1.Type);

                Assert.IsTrue(eventToCommand2.EventCount > 0);
                Assert.AreEqual(bt, eventToCommand2.EventSender);
                Assert.AreEqual(EventToCommandType.SourceObject, eventToCommand2.Type);

                Assert.AreEqual(1, eventToCommand3.EventCount);
                Assert.AreEqual(bt, eventToCommand3.EventSender);
                Assert.AreEqual(EventToCommandType.SourceName, eventToCommand3.Type);

                Assert.AreEqual(1, eventToCommand4.EventCount);
                Assert.AreEqual(root, eventToCommand4.EventSender);
                Assert.AreEqual(EventToCommandType.AssociatedObject, eventToCommand4.Type);

                Assert.AreEqual(1, eventToCommand5.EventCount);
                Assert.AreEqual(bt, eventToCommand5.EventSender);
                Assert.AreEqual(EventToCommandType.SourceObject, eventToCommand5.Type);

                Assert.AreEqual(1, eventToCommand6.EventCount);
                Assert.AreEqual(bt, eventToCommand6.EventSender);
                Assert.AreEqual(EventToCommandType.SourceName, eventToCommand6.Type);
            });
            EnqueueTestComplete();
        }

        void bt_SizeChanged(object sender, SizeChangedEventArgs e) {

        }

        void bt_Loaded(object sender, RoutedEventArgs e) {

        }
#endif
        [Test]
        public void EventArgsConverter_PassEventArgsToCommand() {
            var button = new Button();
            int dataContextChangedCount = 0;
            int dataContextChangedCount2 = 0;
            button.DataContextChanged += (d, e) => dataContextChangedCount2++;
            var eventArgsConverter = new EventArgsConverterTestClass();
            var eventToCommand = new EventToCommand() {
                EventName = "DataContextChanged",
                Command = new DelegateCommand(() => dataContextChangedCount++),
                EventArgsConverter = eventArgsConverter
            };
            Interaction.GetBehaviors(button).Add(eventToCommand);
            Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
            Assert.AreEqual(0, eventArgsConverter.Count);
            button.DataContext = "1";
            Assert.AreEqual(1, dataContextChangedCount);
            Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
            Assert.AreEqual(1, eventArgsConverter.Count);
            eventToCommand.PassEventArgsToCommand = false;
            button.DataContext = "2";
            Assert.AreEqual(2, dataContextChangedCount);
            Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
#if !NETFX_CORE
            Assert.AreEqual(1, eventArgsConverter.Count);
#else
            Assert.AreEqual(2, eventArgsConverter.Count);
            eventToCommand.EventArgsConverter = null;
            button.DataContext = "2";
            Assert.AreEqual(3, dataContextChangedCount);
            Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
            Assert.AreEqual(2, eventArgsConverter.Count);
#endif
        }

#if !NETFX_CORE
        void DispatcherTestCore(Action<EventToCommand> eventToCommandInitializer, bool checkImmediately) {
#else
        async Task DispatcherTestCore(Action<EventToCommand> eventToCommandInitializer, bool checkImmediately) {
#endif
            var button = new Button();
            int dataContextChangedCount = 0;
            int dataContextChangedCount2 = 0;
            button.DataContextChanged += (d, e) => dataContextChangedCount2++;
            var eventToCommand = new EventToCommand() {
                EventName = "DataContextChanged",
                Command = new DelegateCommand(() => dataContextChangedCount++),
#if !SILVERLIGHT && !NETFX_CORE
                DispatcherPriority = DispatcherPriority.Render,
#endif
            };
            eventToCommandInitializer(eventToCommand);
            Interaction.GetBehaviors(button).Add(eventToCommand);
            Window.Content = button;
            Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
            button.DataContext = "1";
            if(!checkImmediately)
                Assert.AreEqual(0, dataContextChangedCount);
            else Assert.AreEqual(1, dataContextChangedCount);
            Assert.AreEqual(1, dataContextChangedCount2);
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
#if !NETFX_CORE
                Assert.AreEqual(1, dataContextChangedCount);
#else
                Assert.AreEqual(dataContextChangedCount2, dataContextChangedCount);
#endif
            });
            EnqueueTestComplete();
        }
#if !SILVERLIGHT && !NETFX_CORE
        [Test]
        public void DispatcherDefaultValues() {
            EventToCommand eventToCommand = new EventToCommand();
            Assert.AreEqual(null, eventToCommand.UseDispatcher);
            Assert.AreEqual(null, eventToCommand.DispatcherPriority);
            Assert.AreEqual(false, eventToCommand.ActualUseDispatcher);
            Assert.AreEqual(DispatcherPriority.Normal, eventToCommand.ActualDispatcherPriority);

            eventToCommand.DispatcherPriority = DispatcherPriority.Normal;
            Assert.AreEqual(true, eventToCommand.ActualUseDispatcher);
            Assert.AreEqual(DispatcherPriority.Normal, eventToCommand.ActualDispatcherPriority);
            eventToCommand.DispatcherPriority = DispatcherPriority.Render;
            Assert.AreEqual(DispatcherPriority.Render, eventToCommand.ActualDispatcherPriority);

            eventToCommand.UseDispatcher = false;
            Assert.AreEqual(false, eventToCommand.ActualUseDispatcher);

            eventToCommand.DispatcherPriority = null;
            eventToCommand.UseDispatcher = true;
            Assert.AreEqual(true, eventToCommand.ActualUseDispatcher);
            Assert.AreEqual(DispatcherPriority.Normal, eventToCommand.ActualDispatcherPriority);
        }
        [Test, Asynchronous]
        public void NotNullDispatcherPriority_NullUseDispatcher() {
            DispatcherTestCore(x => {
                x.DispatcherPriority = DispatcherPriority.Render;
            }, false);
        }
#endif
        [Test, Asynchronous]
#if !NETFX_CORE
        public void TrueUseDispatcher_NullDispatcherPriority() {
#else
        public async Task TrueUseDispatcher_NullDispatcherPriority() {
        await
#endif
            DispatcherTestCore(x => {
                x.UseDispatcher = true;
            }, false);
        }

#if !SILVERLIGHT && !NETFX_CORE
        [Test, Asynchronous]
        public void MarkRoutedEventsAsHandled() {
            var button = new Button() { Name = "View" };
            int counter1 = 0;
            int counter2 = 0;
            int counter3 = 0;
            button.Loaded += (d, e) => counter1++;
            var eventToCommand1 = new EventToCommand() {
                PassEventArgsToCommand = true,
                EventName = "Loaded",
                Command = new DelegateCommand(() => counter2++),
                SourceName = "View",
                MarkRoutedEventsAsHandled = true,
            };
            var eventToCommand2 = new EventToCommand() {
                PassEventArgsToCommand = true,
                EventName = "Loaded",
                Command = new DelegateCommand(() => counter3++),
                SourceName = "View",
                MarkRoutedEventsAsHandled = true,
            };
            Interaction.GetBehaviors(button).Add(eventToCommand1);
            Interaction.GetBehaviors(button).Add(eventToCommand2);
            Window.Content = button;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(1, counter1);
                Assert.AreEqual(1, counter2);
                Assert.AreEqual(0, counter3);
            });
            EnqueueTestComplete();
        }
#endif

        public enum EventToCommandType { AssociatedObject, SourceName, SourceObject }
        public class EventToCommandTestClass : EventToCommand {
            public EventToCommandType Type { get; set; }
            public int EventCount { get; set; }
            public object EventSender { get; set; }
            protected override void OnEvent(object sender, object eventArgs) {
                base.OnEvent(sender, eventArgs);
                EventCount++;
                if(SourceName != null)
                    Type = EventToCommandType.SourceName;
                else if(Source == AssociatedObject)
                    Type = EventToCommandType.AssociatedObject;
                else
                    Type = EventToCommandType.SourceObject;
                EventSender = sender;
            }
        }
        public class EventArgsConverterTestClass : IEventArgsConverter {
            public int Count { get; set; }
            public object Convert(object sender, object args) {
                Count++;
                return null;
            }
        }
    }
}