# DevExpress.Mvvm.Free
DevExpress MVVM Framework is a set of components that simplify the implementation of the Model-View-ViewModel pattern in WPF.

# Documentation
There are two versions of the DevExpress MVVM Framework:
* The DevExpress.Mvvm version is included in the DevExpress WPF component suite and designed for use with the DevExpress WPF controls.
* The DevExpress.Mvvm.Free version is designed for use with the standard or third-party controls.

The DevExpress.Mvvm.Free version is a mirror of the DevExpress.Mvvm framework, so you can use the [documentation](https://documentation.devexpress.com/#WPF/CustomDocument15112) for both versions.

Note that the free version of the framework is not compatible with the DevExpress WPF suite.

# NuGet
The Free DevExpress MVVM Framework is available from [NuGet](https://www.nuget.org/packages/DevExpressMvvm).

# Pull Requests
This repository mirrors the full version of the DevExpress MVVM framework included in DevExpress WPF installation packages. For this reason, we do not accept any pull requests to this repository.
If you have an idea how to improve our MVVM Framework, please contact us via our [support center](https://www.devexpress.com/Support/Center/Question/Create).

# Release Notes

### 22.1.3
* [T1079515](https://supportcenter.devexpress.com/internal/ticket/details/T1079515) - The CompositeCommand and ConfirmationBehavior can work with commands that accept specific type arguments.
* [Notification Service](https://docs.devexpress.com/WPF/18138/mvvm-framework/services/predefined-set/notificationservice#create-a-native-windows-1011-notification) - You can use the optional **Id** parameter of the [NotificationService.CreatePredefinedNotification](https://docs.devexpress.com/WPF/DevExpress.Mvvm.UI.NotificationService.CreatePredefinedNotification(System.String-System.String-System.String-System.Windows.Media.ImageSource-System.String)) method to identify the notification with which the user interacts.
* [Module Manager](https://docs.devexpress.com/WPF/118618/mvvm-framework/mif/module-manager#regions) - You can use our new `ModuleInjection.GetRegions` method to get all the regions from a particular ViewModel.

### 21.1.5
* DevExpress MVVM Framework now includes the [IEventArgsTwoWayConverter](https://docs.devexpress.com/WPF/DevExpress.Mvvm.UI.IEventArgsTwoWayConverter) interface. This interface allows you to define back conversion logic and return values from a command to an event. Refer to the following topic for more information: [EventToCommand](https://docs.devexpress.com/WPF/DevExpress.Mvvm.UI.EventToCommand#pass-a-parameter-back-to-the-bound-event).
### 20.2.3
* [T917390](https://supportcenter.devexpress.com/ticket/details/T917390/the-idelegatecommand-and-iasynccommand-interfaces-have-been-moved-to-the-devexpress-mvvm) - The IDelegateCommand and IAsyncCommand interfaces have been moved to the DevExpress.Mvvm namespace

### 20.1.6
* [T831750](https://supportcenter.devexpress.com/ticket/details/T831750/the-propertymanager-class-has-been-removed)  - The PropertyManager class has been removed
* [T832854](https://supportcenter.devexpress.com/ticket/details/T832854/the-iwindowservice-and-icurrentwindowservice-interfaces-have-been-changed) - The IWindowService and ICurrentWindowService interfaces have been changed
* [T906028](https://supportcenter.devexpress.com/ticket/details/T906028/make-the-bindablebase-setproperty-method-virtual) - Make the BindableBase.SetProperty method virtual

### 19.2.3
* DevExpress MVVM Framework now supports .NET Core 3.
* [T817657](https://www.devexpress.com/Support/Center/Question/Details/T817657/the-idispatcherservice-interface-has-been-changed) - The IDispatcherService interface has been changed

### 18.2.3
* [Async Commands](https://docs.devexpress.com/WPF/17354/mvvm-framework/commands/asynchronous-commands) Enhancements. Our POCO ViewModels and ViewModelBase descendants can now automatically generate Async Commands for methods marked with the async keyword. You can also reference your async method when invalidating an auto-generated Async Command.

```C#
[AsyncCommand(UseCommandManager = false)]
public async Task Calculate() {
    for(int i = 0; i <= 100; i++) {
        Progress = i;
        await Task.Delay(20);
    }
}
void UpdateCalculateCommand() {
    this.RaiseCanExecuteChanged(x => x.Calculate());
}
```

* Dynamic Binding Converters - New API. The new [DelegateConverterFactory](https://docs.devexpress.com/WPF/DevExpress.Mvvm.UI.DelegateConverterFactory) class provides a set of functions to create IValueConverter and IMutliValueConverter instances based on passed delegates.

* Both the [BindableBase](https://docs.devexpress.com/WPF/17350/mvvm-framework/viewmodels/bindablebase) and [ViewModelBase](https://docs.devexpress.com/WPF/17351/mvvm-framework/viewmodels/viewmodelbase) classes now offer a more simplified syntax for getters and setters:

```C#
public string FullName {
    get { return GetValue<string>(); }
    set { SetValue(value, OnFullNameChanged); }
}

string fullName;
public string FullName {
    get { return fullName; }
    set { SetValue(ref fullName, value, OnFullNameChanged); }
}
```

* [CompositeCommandBehavior](https://docs.devexpress.com/WPF/18124/mvvm-framework/behaviors/predefined-set/compositecommandbehavior) - our new CanExecuteCondition property specifies whether the command target should be disabled when the CanExecute method of one of the commands returns true.

### 18.1.3
* [BC4250](https://www.devexpress.com/Support/Center/Question/Details/BC4250/dxbinding-dxcommand-dxevent-moved-to-a-new-engine-with-dynamic-typization) - The [DXBinding, DXEvent, and DXCommand](https://docs.devexpress.com/WPF/115770/mvvm-framework/dxbinding) extensions use a new expression evaluation engine by default. Expressions are now initialized up to 2 times faster. The new Expression Evaluation Engine provides the following features:
  * Dynamic Typing
  * No casting is required to compare or return values
  * Ability to create objects using the ‘new’ operator
  * Ability to assign values using the equals (=) operator in DXCommand and DXEvent

### 16.2.3
* New [Module Injection Framework (MIF)](https://docs.devexpress.com/WPF/118614/mvvm-framework/mif). MIF makes it easier to develop, test, maintain, and deploy modular applications built with loosely coupled modules. The frameworks's key features include:
  * Automatic persistence of the application's logical state and layout.
  * Code separation into logical modules coupled with straightforward navigation.
  * Unit Testing.

### 16.1.4
* The UWP platform is no longer supported. If UWP is required, use the [nuget package version 15.2.5](https://www.nuget.org/packages/DevExpressMvvm/15.2.5).

### 15.2.5
* The Silverlight platform is no longer supported. If Silverlight is required, use the [nuget package version 15.1.4](https://www.nuget.org/packages/DevExpressMvvm/15.1.4).
