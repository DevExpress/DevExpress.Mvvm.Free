using System.Windows.Markup;
using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System;
using System.Security;
using DevExpress.Internal;
using DevExpress.Mvvm.Native;

[assembly: AssemblyTitle("DevExpress.Mvvm.UI")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(AssemblyInfo.AssemblyCompany)]
[assembly: AssemblyProduct("DevExpress.Mvvm.UI")]
[assembly: AssemblyCopyright(AssemblyInfo.AssemblyCopyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: SatelliteContractVersion(AssemblyInfo.SatelliteContractVersion)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: XmlnsDefinition(XmlNamespaceConstants.PresentationNamespaceDefinition, XmlNamespaceConstants.DXBindingNamespace)]
[assembly: XmlnsPrefix(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmPrefix)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmUINamespace)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmInteractivityNamespace)]
[assembly: XmlnsPrefix(XmlNamespaceConstants.MvvmInternalNamespaceDefinition, XmlNamespaceConstants.MvvmIntenalPrefix)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmInternalNamespaceDefinition, XmlNamespaceConstants.MvvmInteractivityInternalNamespace)]

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]

[assembly: InternalsVisibleTo(MvvmAssemblyHelper.TestsFreeAssemblyName)]