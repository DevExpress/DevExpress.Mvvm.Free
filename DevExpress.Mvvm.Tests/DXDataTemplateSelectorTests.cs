using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.DXBinding;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class DXDataTemplateSelectorTests {
        #region templates
        static readonly DataTemplate TextBoxTemplate = (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<TextBox/>
</DataTemplate>");
        static readonly DataTemplate ButtonTemplate = (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<Button/>
</DataTemplate>");
        static readonly DataTemplate CheckBoxTemplate = (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
<CheckBox/>
</DataTemplate>");
        #endregion

        public enum TestEnum { One, Two, Three }
        public class TestData {
            bool @bool;
            public int BoolGetCount;
            [BindableProperty]
            public virtual bool Bool {
                get {
                    BoolGetCount++;
                    return @bool;
                }
                set { @bool = value; }
            }
            public virtual TestEnum Enum { get; set; }
            public virtual string String { get; set; }
        }

        [Test]
        public void Priority() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Enum"),
                Value = "Two",
                Template = ButtonTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });

            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
            Assert.AreEqual(ButtonTemplate, selector.SelectTemplate(new TestData() { Bool = true, Enum = TestEnum.Two }, null));
            Assert.AreEqual(ButtonTemplate, selector.SelectTemplate(new TestData() { Enum = TestEnum.Two }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
        }

        [Test(Description = "T593922")]
        public void ThreadSafety() {
            DXDataTemplateSelector selector = CreateSimpleSelector();
            Action checkSelector = () => {
                Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
                Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
            };
            checkSelector();

            var thread = new Thread(x => checkSelector());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            Assert.AreEqual(ThreadState.Stopped, thread.ThreadState);
        }

        static DXDataTemplateSelector CreateSimpleSelector() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });
            return selector;
        }

        [Test, Explicit]
        public void Performance() {
            MeasureTime(() => {
                var selector = CreateSimpleSelector();
                selector.SelectTemplate(new TestData() { Bool = true }, null);
                selector = CreateSimpleSelector();
                selector.SelectTemplate(new TestData() { Bool = true }, null);
                selector = CreateSimpleSelector();
                selector.SelectTemplate(new TestData() { Bool = true }, null);
            }, "warm up");
            MeasureTime(() => {
                var selector = CreateSimpleSelector();
                selector.SelectTemplate(new TestData() { Bool = true }, null);
            }, "once");
            MeasureTime(() => {
                var selector = CreateSimpleSelector();
                selector.SelectTemplate(new TestData() { Bool = true }, null);
                selector.SelectTemplate(new TestData() { Bool = true }, null);
            }, "twice");
        }

        static void MeasureTime(Action a, string description) {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            a();
            System.Diagnostics.Debug.WriteLine(description + ": " + sw.ElapsedTicks);
        }


        [Test]
        public void UnconvertibleStrings() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "foo",
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Enum"),
                Value = "bar",
                Template = ButtonTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });

            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true, Enum = TestEnum.Two }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData() { Enum = TestEnum.Two }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
        }

        [Test]
        public void RealValues() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = true,
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Enum"),
                Value = TestEnum.Two,
                Template = ButtonTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });

            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
            Assert.AreEqual(ButtonTemplate, selector.SelectTemplate(new TestData() { Bool = true, Enum = TestEnum.Two }, null));
            Assert.AreEqual(ButtonTemplate, selector.SelectTemplate(new TestData() { Enum = TestEnum.Two }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
        }
        [Test]
        public void UseLastTrigger() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = CheckBoxTemplate,
            });

            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
            Assert.AreEqual(null, selector.SelectTemplate(new TestData(), null));
        }
        [Test]
        public void DefaultTemplateFirst() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });

            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));
            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
        }
        [Test]
        public void DefaultTemplateFirst_UseLastDefaultTemplate() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = CheckBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Template = TextBoxTemplate,
            });
            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData(), null));
        }
        [Test]
        public void DoNotListenINPCNotifications() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });
            var data = ViewModelSource.Create<TestData>();

            data.Bool = true;
            Assert.AreEqual(1, data.BoolGetCount);
            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(data, null));
            Assert.AreEqual(2, data.BoolGetCount);

            data.Bool = false;
            DispatcherHelper.DoEvents();
            Assert.AreEqual(3, data.BoolGetCount);
            Assert.AreEqual(null, selector.SelectTemplate(data, null));
            Assert.AreEqual(4, data.BoolGetCount);
        }
        [Test]
        public void NullValue() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Template = TextBoxTemplate,
            });
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("String"),
                Template = CheckBoxTemplate,
            });

            Assert.AreEqual(CheckBoxTemplate, selector.SelectTemplate(new TestData(), null));
            Assert.AreEqual(null, selector.SelectTemplate(new TestData() { String = "" }, null));
        }
        [Test]
        public void NullBinding() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Value = "True",
                Template = TextBoxTemplate,
            });
            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { String = "" }, null));
        }
        [Test]
        public void Freeze() {
            var selector = new DXDataTemplateSelector();
            selector.Items.Add(new DXDataTemplateTrigger() {
                Binding = new Binding("Bool"),
                Value = "True",
                Template = TextBoxTemplate,
            });
            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData() { Bool = true }, null));


            Assert.Throws<InvalidOperationException>(() => selector.Items.Add(new DXDataTemplateTrigger()));
            Assert.Throws<InvalidOperationException>(() => selector.Items[0].Value = null);
            Assert.Throws<InvalidOperationException>(() => selector.Items[0].Binding = null);
            Assert.Throws<InvalidOperationException>(() => selector.Items[0].Template = null);
        }
        [Test]
        public void DontFreezeInDesignTime() {
            var selector = new DXDataTemplateSelector();
            DesignerProperties.SetIsInDesignMode(selector.Items, true);
            selector.Items.Add(new DXDataTemplateTrigger() { Template = TextBoxTemplate });
            Assert.AreEqual(TextBoxTemplate, selector.SelectTemplate(new TestData(), null));

            Assert.IsFalse(selector.Items.IsFrozen);
            selector.Items.Add(new DXDataTemplateTrigger());
        }

    }
}