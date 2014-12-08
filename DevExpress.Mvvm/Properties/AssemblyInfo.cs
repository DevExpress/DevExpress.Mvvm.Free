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

[assembly: AssemblyTitle("DevExpress.Mvvm")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(AssemblyInfo.AssemblyCompany)]
[assembly: AssemblyProduct("DevExpress.Mvvm")]
[assembly: AssemblyCopyright(AssemblyInfo.AssemblyCopyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#if !NETFX_CORE
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]
#endif
[assembly: SatelliteContractVersion(AssemblyInfo.SatelliteContractVersion)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]

#if !NETFX_CORE
[assembly: XmlnsPrefix(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmPrefix)]
[assembly: XmlnsDefinition(XmlNamespaceConstants.MvvmNamespaceDefinition, XmlNamespaceConstants.MvvmNamespace)]
#endif

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]

[assembly: InternalsVisibleTo(MvvmAssemblyHelper.TestsFreeAssemblyName)]
[assembly: InternalsVisibleTo(MvvmAssemblyHelper.MvvmUIAssemblyName)]