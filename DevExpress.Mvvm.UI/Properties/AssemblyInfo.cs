#if !NETFX_CORE
using System.Windows.Markup;
#endif
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
#if !SILVERLIGHT && !NETFX_CORE
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
#endif

#if !NETFX_CORE
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]
#endif

[assembly: SatelliteContractVersion(AssemblyInfo.SatelliteContractVersion)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]

#if !NETFX_CORE
[assembly: XmlnsPrefix(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmPrefix)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmUINamespace)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmInteractivityNamespace)]
[assembly: XmlnsPrefix(XmlNamespaceConstants.MvvmInternalNamespaceDefinition, XmlNamespaceConstants.MvvmIntenalPrefix)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmInternalNamespaceDefinition, XmlNamespaceConstants.MvvmInteractivityInternalNamespace)]
#endif

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]

[assembly: InternalsVisibleTo(MvvmAssemblyHelper.TestsFreeAssemblyName)]