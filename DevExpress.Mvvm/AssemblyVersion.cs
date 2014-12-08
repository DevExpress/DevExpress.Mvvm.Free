namespace DevExpress.Internal {
 internal static class AssemblyInfo {
    public const string AssemblyCopyright = "Copyright (c) 2000-2014 Developer Express Inc.";
    public const string AssemblyCompany = "Developer Express Inc.";

    public const int VersionId = 142;
    public const int VersionIdMinor = 14201;
    public const string VersionShort = "14.2";
#if SILVERLIGHT
    public const string Version = VersionShort + ".0.5";
#elif NETFX_CORE
    public const string Version = VersionShort + ".0.3";
#else
    public const string Version = VersionShort + ".0.0";
#endif
    public const string FileVersion = Version;
    public const string MarketingVersion = "v2014 vol 2";
    public const string VirtDirSuffix = "_v14_2";

    public const string PublicKeyToken = "79868b8147b5eae4";
    public const string PublicKey = "0024000004800000940000000602000000240000525341310004000001000100dd3415ad127e2479d518586804419e99231acd687f889e897fb021bec3d90d53781811bb9d569e032d00362413298930c553dfd43a24e699c6a3d4922824f3c987fc01524b94059de1ccfbef1ff6aedc86055d56c4c3c92c550c84a1410b0c0e891e8f2f0fa193e1532f25727ae634055808129b901bdc24cb517e95fb8815b5";

    public const string FullAssemblyVersionExtension = ", Version=" + Version + ", Culture=neutral, PublicKeyToken=" + PublicKeyToken;

    public const string SatelliteContractVersion = VersionShort + ".0.0";
    public const string VSuffixWithoutSeparator = "v" + VersionShort;
    public const string VSuffix = "." + VSuffixWithoutSeparator;
    public const string VSuffixDesign = VSuffix + ".Design";
    public const string VSuffixExport    = VSuffix + ".Export";
    public const string VSuffixLinq = VSuffix + ".Linq";
    public const string SRAssemblyAgScheduler = "DevExpress.Xpf.Scheduler" + VSuffix;
    public const string SRAssemblyAssemblyLoader = "DevExpress.Xpf.AssemblyLoader" + VSuffix;
    public const string SRAssemblyXpfSpellChecker = "DevExpress.Xpf.SpellChecker" + VSuffix;
    public const string SRAssemblyXpfRichEdit = "DevExpress.Xpf.RichEdit" + VSuffix;
    public const string SRAssemblyXpfSpreadsheet = "DevExpress.Xpf.Spreadsheet" + VSuffix;
    public const string SRAssemblyXpfScheduler = "DevExpress.Xpf.Scheduler" + VSuffix;
    public const string SRAssemblyXpfRichEditExtensions = "DevExpress.Xpf.RichEdit" + VSuffix + ".Extensions";
    public const string SRAssemblyXpfPrintingService = "DevExpress.Xpf.Printing" + VSuffix + ".Service";
    public const string SRAssemblyXpfPrinting = "DevExpress.Xpf.Printing" + VSuffix;
    public const string SRAssemblyXpfReportDesigner = "DevExpress.Xpf.ReportDesigner" + VSuffix;
    public const string SRAssemblyXpfCore = "DevExpress.Xpf.Core" + VSuffix;
    public const string SRAssemblyXpfPdfViewer = "DevExpress.Xpf.PdfViewer" + VSuffix;
    public const string SRAssemblyXpfMvvm = "DevExpress.Mvvm" + VSuffix;
    public const string SRAssemblyXpfMvvmFree = "DevExpress.Mvvm";
    public const string SRAssemblyXpfMvvmUIFree = "DevExpress.Mvvm.UI";
    public const string SRAssemblyXpfRibbon = "DevExpress.Xpf.Ribbon" + VSuffix;
    public const string SRAssemblyXpfNavBar = "DevExpress.Xpf.NavBar" + VSuffix;
    public const string SRAssemblyXpfCoreExtensions = "DevExpress.Xpf.Core" + VSuffix + ".Extensions";
    public const string SRAssemblyXpfDemoBase = "DevExpress.Xpf.DemoBase" + VSuffix;
    public const string SRAssemblyXpfGrid = "DevExpress.Xpf.Grid" + VSuffix;
    public const string SRAssemblyXpfGridCore = "DevExpress.Xpf.Grid" + VSuffix + ".Core";
    public const string SRAssemblyXpfGridExtensions = "DevExpress.Xpf.Grid" + VSuffix + ".Extensions";
    public const string SRAssemblyXpfDocking = "DevExpress.Xpf.Docking" + VSuffix;
    public const string SRAssemblyXpfControls = "DevExpress.Xpf.Controls" + VSuffix;
    public const string SRAssemblyXpfLayoutControl = "DevExpress.Xpf.LayoutControl" + VSuffix;
    public const string SRAssemblyImages = "DevExpress.Images" + VSuffix;
    public const string SRAssemblyImagesFull = "DevExpress.Images" + VSuffix + FullAssemblyVersionExtension;
#if !SILVERLIGHT
    public const string SRAssemblyXpfDockingExtensions = "DevExpress.Xpf.Docking" + VSuffix + ".Extensions";
#endif
    public const string SRAssemblyXpfLayoutCore = "DevExpress.Xpf.Layout" + VSuffix + ".Core";
    public const string SRAssemblyDXCharts = "DevExpress.Xpf.Charts" + VSuffix;
#if !SILVERLIGHT
    public const string SRAssemblyChartDesigner = "DevExpress.Charts.Designer" + VSuffix;
#endif
    public const string SRAssemblyDXGauges = "DevExpress.Xpf.Gauges" + VSuffix;
    public const string SRAssemblyDXMap = "DevExpress.Xpf.Map" + VSuffix;

    public const string SRAssemblyData = "DevExpress.Data" + VSuffix;
    public const string SRAssemblyDemoDataCore = "DevExpress.DemoData" + VSuffix + ".Core";
    public const string SRAssemblyPrintingCore = "DevExpress.Printing" + VSuffix + ".Core";
    public const string SRAssemblyRichEditCore = "DevExpress.RichEdit" + VSuffix + ".Core";
    public const string SRAssemblyOfficeCore = "DevExpress.Office" + VSuffix + ".Core";
    public const string SRAssemblyDocs = "DevExpress.Docs" + VSuffix;
    public const string SRAssemblySpreadsheetCore = "DevExpress.Spreadsheet" + VSuffix + ".Core";
    public const string SRAssemblySchedulerCore = "DevExpress.XtraScheduler" + VSuffix + ".Core";
    public const string SRAssemblyReports = "DevExpress.XtraReports" + VSuffix;
    public const string SRAssemblyPrintingDesign = "DevExpress.XtraPrinting" + VSuffixDesign;

    public const string SRDocumentationLink = "http://documentation.devexpress.com/";
    public const string InstallationRegistryKeyBase = "SOFTWARE\\DevExpress\\Components\\";
    public const string InstallationRegistryKey = InstallationRegistryKeyBase + VSuffixWithoutSeparator;
    public const string InstallationRegistryRootPathValueName = "RootDirectory";
    public const string SRAssemblyXpfPrefix = "DevExpress.Xpf";
    public const string ThemePrefixWithoutSeparator = "Themes";
    public const string ThemePrefix = "." + ThemePrefixWithoutSeparator + ".";
#if !SILVERLIGHT
    public const string
        SRAssemblyMVC = "DevExpress.Web.Mvc" + VSuffix,
        SRAssemblyMVC5 = "DevExpress.Web.Mvc5" + VSuffix,
        SRAssemblyExpressAppWeb = "DevExpress.ExpressApp.Web" + VSuffix,
        SRAssemblyExpressAppNotificationsWeb = "DevExpress.ExpressApp.Notifications.Web" + VSuffix,
        SRAssemblyASPxThemes = "DevExpress.Web.ASPxThemes" + VSuffix,
        SRAssemblyASPxThemesFull = SRAssemblyASPxThemes + FullAssemblyVersionExtension,
        SRAssemblyASPxGridView = "DevExpress.Web" + VSuffix,
        SRAssemblyASPxGridViewExport = "DevExpress.Web" + VSuffixExport,
        SRAssemblyASPxPivotGrid = "DevExpress.Web.ASPxPivotGrid" + VSuffix,
        SRAssemblyASPxPivotGridExport = "DevExpress.Web.ASPxPivotGrid" + VSuffixExport,
        SRAssemblyBonusSkins = "DevExpress.BonusSkins" + VSuffix,
        SRAssemblyBonusSkinsFull = "DevExpress.BonusSkins" + VSuffix + FullAssemblyVersionExtension,
        SRAssemblyDesign = "DevExpress.Design" + VSuffix,
        SRAssemblyDesignFull = "DevExpress.Design" + VSuffix + FullAssemblyVersionExtension,
        SRAssemblyDataLinq = "DevExpress.Data" + VSuffixLinq,
        SRAssemblyUtils = "DevExpress.Utils" + VSuffix,
        SRAssemblyParser = "DevExpress.Parser" + VSuffix,
        SRAssemblyPrinting = "DevExpress.XtraPrinting" + VSuffix,
        SRAssemblyEditors = "DevExpress.XtraEditors" + VSuffix,
        SRAssemblyEditorsDesign = "DevExpress.XtraEditors" + VSuffixDesign,
        SRAssemblyEditorsDesignFull = "DevExpress.XtraEditors" + VSuffixDesign + FullAssemblyVersionExtension,
        SRAssemblyNavBar = "DevExpress.XtraNavBar" + VSuffix,
        SRAssemblyNavBarDesign = "DevExpress.XtraNavBar" + VSuffixDesign,
        SRAssemblyBars = "DevExpress.XtraBars" + VSuffix,
        SRAssemblyBarsDesign = "DevExpress.XtraBars" + VSuffixDesign,
        SRAssemblyBarsDesignFull = "DevExpress.XtraBars" + VSuffixDesign + FullAssemblyVersionExtension,
        SRAssemblyGrid = "DevExpress.XtraGrid" + VSuffix,
        SRAssemblyGaugesCore = "DevExpress.XtraGauges" + VSuffix + ".Core",
        SRAssemblyGaugesPresets = "DevExpress.XtraGauges" + VSuffix + ".Presets",
        SRAssemblyGaugesWin = "DevExpress.XtraGauges" + VSuffix + ".Win",
        SRAssemblyASPxGauges = "DevExpress.Web.ASPxGauges" + VSuffix,
        SRAssemblyGaugesDesignWin = "DevExpress.XtraGauges" + VSuffixDesign + ".Win",
        SRAssemblyGridDesign = "DevExpress.XtraGrid" + VSuffixDesign,
        SRAssemblyPivotGrid = "DevExpress.XtraPivotGrid" + VSuffix,
        SRAssemblyPivotGridCore = "DevExpress.PivotGrid" + VSuffix + ".Core",
        SRAssemblyPivotGridDesign = "DevExpress.XtraPivotGrid" + VSuffixDesign,
        SRAssemblyPivotGridDesignFull = "DevExpress.XtraPivotGrid" + VSuffixDesign + FullAssemblyVersionExtension,
        SRAssemblyTreeList = "DevExpress.XtraTreeList" + VSuffix,
        SRAssemblyTreeListDesign = "DevExpress.XtraTreeList" + VSuffixDesign,
        SRAssemblyVertGrid = "DevExpress.XtraVerticalGrid" + VSuffix,
        SRAssemblyVertGridDesign = "DevExpress.XtraVerticalGrid" + VSuffixDesign,
        SRAssemblyReportsService = "DevExpress.XtraReports" + VSuffix + ".Service",
        SRAssemblyReportsDesign = "DevExpress.XtraReports" + VSuffixDesign,
        SRAssemblyReportsImport = "DevExpress.XtraReports" + VSuffix + ".Import",
        SRAssemblyReportsWeb = "DevExpress.XtraReports" + VSuffix + ".Web",
        SRAssemblyReportsExtensions = "DevExpress.XtraReports" + VSuffix + ".Extensions",
        SRAssemblyReportServerDesigner = "DevExpress.ReportDesigner" + VSuffix + ".Core",
        SRAssemblyReportServerWeb = "DevExpress.ReportServer" + VSuffix + ".Web",
        SRAssemblyRichEdit = "DevExpress.XtraRichEdit" + VSuffix,
        SRAssemblyRichEditDesign = "DevExpress.XtraRichEdit" + VSuffixDesign,
        SRAssemblyRichEditExtensions = "DevExpress.XtraRichEdit" + VSuffix + ".Extensions",
        SRAssemblySpreadsheet = "DevExpress.XtraSpreadsheet" + VSuffix,
        SRAssemblySpreadsheetDesign = "DevExpress.XtraSpreadsheet" + VSuffixDesign,
        SRAssemblyScheduler = "DevExpress.XtraScheduler" + VSuffix,
        SRAssemblySchedulerDesign = "DevExpress.XtraScheduler" + VSuffixDesign,
        SRAssemblySchedulerWeb = "DevExpress.Web.ASPxScheduler" + VSuffix,
        SRAssemblySchedulerWebDesign = "DevExpress.Web.ASPxScheduler" + VSuffixDesign,
        SRAssemblySchedulerWebDesignFull = "DevExpress.Web.ASPxScheduler" + VSuffixDesign + FullAssemblyVersionExtension,
        SRAssemblySchedulerExtensions = "DevExpress.XtraScheduler" + VSuffix + ".Extensions",
        SRAssemblySchedulerReporting = "DevExpress.XtraScheduler" + VSuffix + ".Reporting",
        SRAssemblySchedulerReportingExtensions = "DevExpress.XtraScheduler" + VSuffix + ".Reporting.Extensions",
        SRAssemblyChartsCore = "DevExpress.Charts" + VSuffix + ".Core",
        SRAssemblySparklineCore = "DevExpress.Sparkline" + VSuffix + ".Core",
        SRAssemblyCharts = "DevExpress.XtraCharts" + VSuffix,
        SRAssemblyChartsExtensions = "DevExpress.XtraCharts" + VSuffix + ".Extensions",
        SRAssemblyChartsWizard = "DevExpress.XtraCharts" + VSuffix + ".Wizard" + FullAssemblyVersionExtension,
        SRAssemblyChartsDesign = "DevExpress.XtraCharts" + VSuffixDesign,
        SRAssemblyChartsWebDesign = "DevExpress.XtraCharts" + VSuffix + ".Web.Design" + FullAssemblyVersionExtension,
        SRAssemblyChartsUI = "DevExpress.XtraCharts" + VSuffix + ".UI",
        SRAssemblyChartsWeb = "DevExpress.XtraCharts" + VSuffix + ".Web",
        SRAssemblyWizard = "DevExpress.XtraWizard" + VSuffix,
        SRAssemblyWizardDesign = "DevExpress.XtraWizard" + VSuffixDesign,
        SRAssemblyXpo = "DevExpress.Xpo" + VSuffix,
        SRAssemblyXpoDesign = "DevExpress.Xpo" + VSuffixDesign,
        SRAssemblyXpoDesignFull = SRAssemblyXpoDesign + FullAssemblyVersionExtension,
        SRAssemblyLayoutControl = "DevExpress.XtraLayout" + VSuffix,
        SRAssemblyLayoutControlDesign = "DevExpress.XtraLayout" + VSuffixDesign,
        SRAssemblySpellCheckerCore = "DevExpress.SpellChecker" + VSuffix + ".Core",
        SRAssemblySpellChecker = "DevExpress.XtraSpellChecker" + VSuffix,
        SRAssemblySpellCheckerDesign = "DevExpress.XtraSpellChecker" + VSuffixDesign,
        SRAssemblySpellCheckerDesignFull = SRAssemblySpellCheckerDesign + FullAssemblyVersionExtension,
        SRAssemblySpellCheckerWeb = "DevExpress.Web.ASPxSpellChecker" + VSuffix,
        SRAssemblyWeb = "DevExpress.Web" + VSuffix,
        SRAssemblyWebDesign = "DevExpress.Web" + VSuffixDesign,
        SRAssemblyWebDesignFull = "DevExpress.Web" + VSuffixDesign + FullAssemblyVersionExtension,
        SRAssemblyWebLinq = "DevExpress.Web" + VSuffixLinq,
        SRAssemblyWebSpreadsheet = "DevExpress.Web.ASPxSpreadsheet" + VSuffix,
        SRAssemblyWebRichEdit = "DevExpress.Web.ASPxRichEdit" + VSuffix,
        SRAssemblyWebRichEditTests = "DevExpress.Web.ASPxRichEdit" + VSuffix + ".Tests",

        SRAssemblyHtmlEditorWeb = "DevExpress.Web.ASPxHtmlEditor" + VSuffix,
        SRAssemblyEditorsWeb = "DevExpress.Web" + VSuffix,
        SRAssemblyTreeListWeb = "DevExpress.Web.ASPxTreeList" + VSuffix,
        SRAssemblyTreeListWebExport = "DevExpress.Web.ASPxTreeList" + VSuffixExport,
        SRAssemblyDXPivotGrid = "DevExpress.Xpf.PivotGrid" + VSuffix,
        SRAssemblyDXThemeEditorUIWithoutVSuffix = "DevExpress.Xpf.ThemeEditor",
        SRAssemblyDXThemeEditorUI = SRAssemblyDXThemeEditorUIWithoutVSuffix + VSuffix,

        SRAssemblySnap = "DevExpress.Snap" + VSuffix,
        SRAssemblySnapCore = "DevExpress.Snap" + VSuffix + ".Core",
        SRAssemblySnapExtensions = "DevExpress.Snap" + VSuffix + ".Extensions",
        SRAssemblySnapDesign = "DevExpress.Snap" + VSuffixDesign,

        SRAssemblyUtilsUI = "DevExpress.Utils" + VSuffix + ".UI",
        SRAssemblyDashboardCore = "DevExpress.Dashboard" + VSuffix + ".Core",
        SRAssemblyDashboardWin = "DevExpress.Dashboard" + VSuffix + ".Win",
        SRAssemblyDashboardWeb = "DevExpress.Dashboard" + VSuffix + ".Web",
        SRAssemblyDashboardWebMVC = "DevExpress.Dashboard" + VSuffix + ".Web.Mvc",
        SRAssemblyDashboardWebMVC5 = "DevExpress.Dashboard" + VSuffix + ".Web.Mvc5",
        SRAssemblyDashboardWinDesign = "DevExpress.Dashboard" + VSuffix + ".Win.Design",
        SRAssemblyDashboardWebDesign = "DevExpress.Dashboard" + VSuffix + ".Web.Design" + FullAssemblyVersionExtension,
        SRAssemblyMapCore = "DevExpress.Map" + VSuffix + ".Core",
        SRAssemblyMap = "DevExpress.XtraMap" + VSuffix,
        SRAssemblyMapDesign = "DevExpress.XtraMap" + VSuffixDesign,
        SRAssemblyDataAccess = "DevExpress.DataAccess" + VSuffix,
        SRAssemblyDataAccessUI = "DevExpress.DataAccess" + VSuffix + ".UI",
        SRAssemblyDataAccessDesign = "DevExpress.DataAccess" + VSuffix + ".Design",
        SRAssemblyDataAccessDesignFull = SRAssemblyDataAccessDesign + FullAssemblyVersionExtension,
        SRAssemblyPdfCore = "DevExpress.Pdf" + VSuffix + ".Core",
        SRAssemblyPdfDrawing = "DevExpress.Pdf" + VSuffix + ".Drawing",
        SRAssemblyXtraPdfViewer = "DevExpress.XtraPdfViewer" + VSuffix,
        SRAssemblyXtraPdfViewerDesign = "DevExpress.XtraPdfViewer" + VSuffix + ".Design";
#endif
    public const string
        DXTabNameComponents = "Components",
        DXTabNameNavigationAndLayout = "Navigation & Layout",
        DXTabNameOrmComponents = "ORM Components",
        DXTabNameReporting = "Reporting",
        DXTabNameReportControls = "Report Controls",
        DXTabNameDashboardItems = "Dashboard Items",
        DXTabNameData = "Data & Analytics",
        DXTabNameVisualization = "Visualization",
        DXTabNameScheduling = "Scheduling",
        DXTabNameSchedulerReporting = "Scheduler Reporting",
        DXTabNameRichEdit = "Rich Text Editor",
        DXTabNameSpreadsheet = "Spreadsheet",
        DXTabNameCommon = "Common Controls",
        DXTabNameLayoutControl = "Layout Control",
        DXTabPrefix = "DX." + VersionShort + ": ",
        DXTabFree = DXTabPrefix + "Free",
        DXTabComponents = DXTabPrefix + DXTabNameComponents,
        DXTabNavigation = DXTabPrefix + DXTabNameNavigationAndLayout,
        DXTabReporting = DXTabPrefix + DXTabNameReporting,
        DXTabReportControls = DXTabPrefix + DXTabNameReportControls,
        DXTabDashboardItems = DXTabPrefix + DXTabNameDashboardItems,
        DXTabData = DXTabPrefix + DXTabNameData,
        DXTabVisualization = DXTabPrefix + DXTabNameVisualization,
        DXTabOrmComponents = DXTabPrefix + DXTabNameOrmComponents,
        DXTabScheduling = DXTabPrefix + DXTabNameScheduling,
        DXTabSchedulerReporting = DXTabPrefix + DXTabNameSchedulerReporting,
        DXTabRichEdit = DXTabPrefix + DXTabNameRichEdit,
        DXTabSpreadsheet = DXTabPrefix + DXTabNameSpreadsheet,
        DXTabCommon = DXTabPrefix + DXTabNameCommon,
        DXTabNameXPOProfiler = "XPO " + VersionShort + " Profiler";

    public const string
        DXTabWpfNavigation = "DX." + VersionShort + ": Navigation & Layout",
        DXTabWpfReporting = "DX." + VersionShort + ": Reporting",
        DXTabWpfData = "DX." + VersionShort + ": Data",
        DXTabWpfVisualization = "DX." + VersionShort + ": Visualization",
        DXTabWpfCommon = "DX." + VersionShort + ": Common Controls",
        DXTabWpfScheduling = "DX." + VersionShort + ": Scheduling",
        DXTabWpfRichEdit = "DX." + VersionShort + ": Rich Text Editor",
        DXTabWpfSpreadsheet = "DX." + VersionShort + ": Spreadsheet";

    public const string
        DXLinkCompetitiveDiscounts = "https://go.devexpress.com/Demo_2013_Competitive_Discounts.aspx",
        DXLinkCompare = "https://go.devexpress.com/Demo_2013_CompareSubscriptions.aspx",
        DXLinkTrial = "https://go.devexpress.com/Demo_2013_DownloadTrial.aspx",
        DXLinkChat = "Https://go.devexpress.com/Demo_2013_Chat.aspx",
        DXLinkHelp = "Https://go.devexpress.com/Demo_2013_Help.aspx",
        DXLinkBuyNow = "Https://go.devexpress.com/Demo_2013_BuyNow.aspx",
        DXLinkBuyNowASP = "https://go.devexpress.com/Demo_2013_BuyNow_ASP.aspx",
        DXLinkWhatsNew = "Https://go.devexpress.com/Demo_2013_13_2_WhatsNew.aspx",
        DXLinkGetSupport = "Https://go.devexpress.com/Demo_2013_GetSupport.aspx",
        DXLinkGetStarted = "Https://go.devexpress.com/Demo_2013_GetStartedOverall.aspx",
        DXLinkRegisterKB = "https://go.devexpress.com/Demo_2013_RegisterTrial.aspx",
        DXLinkGetStartedWinGrid = "https://go.devexpress.com/Demo_2013_GetStartedWinGrid.aspx",
        DXEmailInfo = "info@devexpress.com",
        DXLinkEmailInfo = "mailto:" + DXEmailInfo;

}
}