using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Example.ViewModel {
    public enum ExportSettings {
        [Image("pack://application:,,,/Images/ExportToPdf.png")]
        [Display(Description = "Export file to .pdf format")]
        PdfExport,
        [Image("pack://application:,,,/Images/ExportToHtml.png")]
        [Display(Description = "Export file to .html format")]
        HtmlExport,
        [Image("pack://application:,,,/Images/ExportToDoc.png")]
        [Display(Description = "Export file to .doc format", Name = "DocExport (MSOffice 2003)")]
        DocExport
    }
    public class EnumWithExternalMetadata {
        public static void BuildMetadata(EnumMetadataBuilder<HorizontalAlignment> builder) {
            builder
                .Member(HorizontalAlignment.Center)
                    .Description("Centered")
                    .ImageUri("pack://application:,,,/Images/Centered.png")
                .EndMember()
                .Member(HorizontalAlignment.Left)
                    .Description("Align Left")
                    .ImageUri("pack://application:,,,/Images/AlignLeft.png")
                .EndMember()
                .Member(HorizontalAlignment.Right)
                    .Description("Align Right")
                    .ImageUri("pack://application:,,,/Images/AlignRight.png")
                .EndMember()
                .Member(HorizontalAlignment.Stretch)
                    .Description("Stretched")
                    .ImageUri("pack://application:,,,/Images/Stretched.png")
                .EndMember();
        }
    }

    [POCOViewModel]
    public class MainViewModel {
        public MainViewModel() {
            MetadataLocator.Default = MetadataLocator.Create().AddMetadata<EnumWithExternalMetadata>();
        }
    }
}

