using System;
using System.Windows.Input;

using System.Windows.Controls;

namespace DevExpress.Mvvm.UI.Tests {
    public class EventToCommandTestViewModel : BindableBase {
        public class DummyCommandClass : ICommand {
            bool ICommand.CanExecute(object parameter) {
                return false;
            }
            event EventHandler ICommand.CanExecuteChanged {
                add { }
                remove { }
            }
            void ICommand.Execute(object parameter) {
                InvokeCount++;
            }
            public int InvokeCount { get; private set; }
        }

        string testParameter = "test";

        public string TestParameter {
            get { return testParameter; }
            set { SetProperty(ref testParameter, value, () => TestParameter); }
        }


        public int LoadedCount { get; private set; }
        public string LoadedParameter { get; set; }
        ICommand loadedCommand;
        public ICommand LoadedCommand {
            get {
                if(loadedCommand == null) {
                    loadedCommand = new DelegateCommand<string>(o => {
                        LoadedCount++;
                        LoadedParameter = o;
                    });
                }
                return loadedCommand;
            }
        }

        public int ButtonLoadedCount { get; private set; }
        ICommand buttonLoadedCommand;
        public ICommand ButtonLoadedCommand {
            get {
                if(buttonLoadedCommand == null) {
                    buttonLoadedCommand = new DelegateCommand<string>(o => {
                        ButtonLoadedCount++;
                    });
                }
                return buttonLoadedCommand;
            }
        }

        DummyCommandClass dummyCommand;
        public DummyCommandClass DummyCommand {
            get {
                if(dummyCommand == null) {
                    dummyCommand = new DummyCommandClass();
                }
                return dummyCommand;
            }
        }



        public int SelectionChangedCount { get; private set; }
        public SelectionChangedEventArgs SelectionChangedParameter { get; set; }
        ICommand selectionChangedCommand;
        public ICommand SelectionChangedCommand {
            get {
                if(selectionChangedCommand == null) {
                    selectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(o => {
                        SelectionChangedCount++;
                        SelectionChangedParameter = o;
                    });
                }
                return selectionChangedCommand;
            }
        }

        public int SelectionChangedCount2 { get; private set; }
        public object SelectionChangedParameter2 { get; set; }
        ICommand selectionChangedCommand2;
        public ICommand SelectionChangedCommand2 {
            get {
                if(selectionChangedCommand2 == null) {
                    selectionChangedCommand2 = new DelegateCommand<object>(o => {
                        SelectionChangedCount2++;
                        SelectionChangedParameter2 = o;
                    });
                }
                return selectionChangedCommand2;
            }
        }

        public int SelectionChangedCount3 { get; private set; }
        public object SelectionChangedParameter3 { get; set; }
        ICommand selectionChangedCommand3;
        public ICommand SelectionChangedCommand3 {
            get {
                if(selectionChangedCommand3 == null) {
                    selectionChangedCommand3 = new DelegateCommand<object>(o => {
                        SelectionChangedCount3++;
                        SelectionChangedParameter3 = o;
                    });
                }
                return selectionChangedCommand3;
            }
        }

        bool selectionChangedCommandParameter4 = false;
        public bool SelectionChangedCommandParameter4 {
            get { return selectionChangedCommandParameter4; }
            set { SetProperty(ref selectionChangedCommandParameter4, value, () => SelectionChangedCommandParameter4); }
        }
        public int SelectionChangedCount4 { get; private set; }
        ICommand selectionChangedCommand4;
        public ICommand SelectionChangedCommand4 {
            get {
                if(selectionChangedCommand4 == null) {
                    selectionChangedCommand4 = new DelegateCommand<bool>(o => {
                        SelectionChangedCount4++;
                    }, o => {
                        return o;
                    });
                }
                return selectionChangedCommand4;
            }
        }

        bool selectionChangedCommandParameter5 = false;
        public bool SelectionChangedCommandParameter5 {
            get { return selectionChangedCommandParameter5; }
            set { SetProperty(ref selectionChangedCommandParameter5, value, () => SelectionChangedCommandParameter5); }
        }
        public int SelectionChangedCount5 { get; private set; }
        ICommand selectionChangedCommand5;
        public ICommand SelectionChangedCommand5 {
            get {
                if(selectionChangedCommand5 == null) {
                    selectionChangedCommand5 = new DelegateCommand<bool>(o => {
                        SelectionChangedCount5++;
                    }, o => {
                        return o;
                    });
                }
                return selectionChangedCommand5;
            }
        }

        public int Q554072CommandCount { get; set; }
        ICommand q554072Command;
        public ICommand Q554072Command {
            get {
                if(q554072Command == null) {
                    q554072Command = new DelegateCommand(() => {
                        Q554072CommandCount++;
                    });
                }
                return q554072Command;
            }
        }

    }
    public class SelectionChangedConverter : IEventArgsConverter {
        object IEventArgsConverter.Convert(object sender, object args) {
            if(args is SelectionChangedEventArgs)
                return ((SelectionChangedEventArgs)args).AddedItems[0];
            return null;
        }
    }
}