using DevExpress.DXBinding.Native;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Xpf.DXBinding.Tests {
    public class BaseTestFixtureBase {
        internal class BindingTreeInfoTest : BindingTreeInfo {
            public BindingTreeInfoTest(string input) : this(input, null) { }
            public BindingTreeInfoTest(string input, string backInput)
                : base(input, backInput, null) { }
            public string ExprToString() {
                return VisitorString.Resolve(Expr);
            }
            public string BackExprToString() {
                return VisitorString.Resolve(BackExpr);
            }
            protected override void Throw(string msg) {
                Assert.Fail(msg);
            }
        }
        internal class CommandTreeInfoTest : CommandTreeInfo {
            public CommandTreeInfoTest(string input, string canInput) : base(input, canInput, null) { }
            public string ExecuteExprToString() {
                return VisitorString.Resolve(ExecuteExpr);
            }
            public string CanExecuteExprToString() {
                return VisitorString.Resolve(CanExecuteExpr);
            }
            protected override void Throw(string msg) {
                Assert.Fail(msg);
            }
        }
        internal class TestTypeResolver : ITypeResolver {
            readonly Func<string, Type> func;
            public TestTypeResolver(Func<string, Type> func) {
                this.func = func;
            }
            Type ITypeResolver.ResolveType(string type) {
                return func(type);
            }
        }
        internal class TestErrorHandler : ErrorHandlerBase {
            readonly bool fail;
            public TestErrorHandler(bool fail = false) {
                this.fail = fail;
            }
            public string ErrorMsg { get; private set; }

            protected override void ReportCore(string msg) {
                ErrorMsg = msg;
            }
            protected override void ThrowCore(string msg, Exception innerException) {
                ErrorMsg = !string.IsNullOrEmpty(msg) ? msg : innerException.Message;
                if(fail) Assert.Fail();
            }
        }

        internal void AssertSyntaxTreeString(string input, string expected = null) {
            AssertSyntaxTreeString(input, null, expected);
        }
        internal void BackMode_AssertSyntaxTreeString(string input, string expected = null) {
            AssertSyntaxTreeString(null, input, expected);
        }
        internal void Execute_AssertSyntaxTreeString(string input, string expected = null) {
            var info = new CommandTreeInfoTest(input, null);
            Assert.AreEqual(expected ?? input, info.ExecuteExprToString());
        }
        internal void CanExecute_AssertSyntaxTreeString(string input, string expected = null) {
            var info = new CommandTreeInfoTest(null, input);
            Assert.AreEqual(expected ?? input, info.CanExecuteExprToString());
        }
        static void AssertSyntaxTreeString(string input, string backInput, string expected) {
            var info = new BindingTreeInfoTest(input, backInput);
            if (input != null) {
                if (expected == null)
                    expected = input.Replace("$", "");
                Assert.AreEqual(expected ?? input, info.ExprToString());
            }
            if (backInput != null) {
                if (expected == null)
                    expected = backInput.Replace("$", "");
                Assert.AreEqual(expected ?? backInput, info.BackExprToString());
            }
        }

        internal void AssertIdent(string input, string expectedValue = null) {
            var info = new BindingTreeInfoTest(input);
            var n = (NIdent)info.Expr.Expr;
            Assert.AreEqual(expectedValue ?? input, info.ExprToString());
        }
        internal void AssertConstant(string input, NConstant.NKind expectedType, object expectedValue, bool checkSyntaxTreeString = true) {
            var info = new BindingTreeInfoTest(input);
            var n = (NConstant)info.Expr.Expr;
            Assert.AreEqual(expectedType, n.Kind);
            Assert.AreEqual(expectedValue, n.Value);
            if(expectedValue != null) Assert.AreEqual(expectedValue.GetType(), n.Value.GetType());
            if(checkSyntaxTreeString && expectedValue != null)
                AssertSyntaxTreeString(input, expectedValue.ToString());
        }

        internal void AssertOperands(string input, string operandsOrder) {
            var info = new BindingTreeInfoTest(input);
            var operands = GetOperands(input, null).Select(x => x.Path);
            var expOperands = operandsOrder == null ? new string[] { } : operandsOrder.Split(';');
            Assert.AreEqual(expOperands.Count(), operands.Count());
            for(int i = 0; i < operands.Count(); i++)
                Assert.AreEqual(expOperands.ElementAt(i), operands.ElementAt(i));
        }
        internal void AssertOperand(Operand operand, string expectedPath, bool shouldBeTwoWay) {
            Assert.AreEqual(expectedPath, operand.Path);
            Assert.AreEqual(shouldBeTwoWay, operand.IsTwoWay);
        }
        internal IEnumerable<Operand> GetOperands(string input, string backInput) {
            var info = new BindingTreeInfoTest(input, backInput);
            var errorHandler = new TestErrorHandler();
            var res = VisitorOperand.Resolve(new[] { info.Expr }, info.BackExpr, x => typeof(object), errorHandler);
            if(errorHandler.HasError) Assert.Fail();
            return res;
        }

        internal void AssertArithmetic(string input, object expected, string operandsOrder = null, Array operands = null, Func<string, Type> typeResolver = null) {
            AssertArithmetic(input, res => {
                Assert.AreEqual(expected, res);
                if (expected != null) Assert.AreEqual(expected.GetType(), res.GetType());
            }, operandsOrder, operands, typeResolver);
        }
        internal void AssertArithmetic(string input, Action<object> checkResult, string operandsOrder = null, Array operands = null, Func<string, Type> typeResolver = null) {
            if (operandsOrder != null) AssertOperands(input, operandsOrder);
            var info = new BindingTreeInfoTest(input);
            object[] ops = operands == null ? null : operands.Cast<object>().ToArray();
            var calculator = CreateBindingCalculator(info, new TestErrorHandler(true));
            calculator.Init(new TestTypeResolver(typeResolver));
            var res = calculator.Resolve(ops);
            if (checkResult != null)
                checkResult(res);
        }
        internal void Execute_AssertArithmetic(string input, object[] opValues, object parameter, Func<string, Type> typeResolver = null) {
            var info = new CommandTreeInfoTest(input, null);
            var calculator = CreateCommandCalculator(info, new TestErrorHandler(true));
            calculator.Init(new TestTypeResolver(typeResolver));
            calculator.Execute(opValues, parameter);
        }

        internal void AssertValidation(string input, object expected, Array operands, string message, Func<string, Type> typeResolver = null) {
            var info = new BindingTreeInfoTest(input);
            var errorHandler = new TestErrorHandler();
            var calculator = CreateBindingCalculator(info, errorHandler);
            calculator.Init(new TestTypeResolver(typeResolver));
            var res = calculator.Resolve(operands.Cast<object>().ToArray());
            Assert.AreEqual(expected, res);
            if(expected != null) Assert.AreEqual(expected.GetType(), res.GetType());
            Assert.AreEqual(message, errorHandler.ErrorMsg);
        }
        internal void Execute_AssertValidation(string input, Array operands, string message, Func<string, Type> typeResolver = null) {
            var info = new CommandTreeInfoTest(input, null);
            var errorHandler = new TestErrorHandler();
            var calculator = CreateCommandCalculator(info, errorHandler);
            calculator.Init(new TestTypeResolver(typeResolver));
            calculator.Execute(operands.Cast<object>().ToArray(), null);
            Assert.AreEqual(message, errorHandler.ErrorMsg);
        }

        internal virtual IBindingCalculator CreateBindingCalculator(BindingTreeInfoTest info, TestErrorHandler errorHandler) {
            return new BindingCalculator(info.Expr, info.BackExpr, null, errorHandler);
        }
        internal virtual ICommandCalculator CreateCommandCalculator(CommandTreeInfo info, TestErrorHandler errorHandler) {
            return new CommandCalculator(info.ExecuteExpr, info.CanExecuteExpr, true, errorHandler);
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BaseParserTests : BaseTestFixtureBase {
        [Test]
        public virtual void Property() {
            AssertIdent("a");
            AssertIdent("a1");
            AssertIdent("a.b");
            AssertIdent("_._");
            AssertIdent("_1._1");
            AssertIdent("_1a._1a");
            AssertSyntaxTreeString("-a");
            AssertSyntaxTreeString("--+a.b", "--a.b");
            AssertSyntaxTreeString("(sbyte1)+1", "sbyte1+1");
            AssertSyntaxTreeString("`abc`.Length", "(\"abc\").Length");

            AssertIdent("isa");
            AssertIdent("bytes");

            AssertArithmetic("a.GetSelf().AtProp", 1, null, new[] { new BaseParserTests_a(intV: 1) });
            AssertArithmetic("a.GetSelf().int", 1, null, new[] { new BaseParserTests_a(intV: 1) });
        }
        [Test]
        public virtual void Method() {
            AssertSyntaxTreeString("Method1()");
            AssertSyntaxTreeString("Method1().Method2()");
            AssertSyntaxTreeString("Method1().Prop1.Method2()");
            AssertSyntaxTreeString("Method1().Prop1.Method2().Prop2");
            AssertSyntaxTreeString("Prop1");
            AssertSyntaxTreeString("Prop1.Prop2");
            AssertSyntaxTreeString("Prop1.Method1().Prop2");
            AssertSyntaxTreeString("Prop1.Method1().Prop2.Method2()");

            AssertSyntaxTreeString("Method1(arg1)");
            AssertSyntaxTreeString("Method1(arg1,arg2)");
            AssertSyntaxTreeString("Method1(1+2 ,-arg2)", "Method1(1+2,-arg2)");

            AssertSyntaxTreeString("Method1(arg1)");
            AssertSyntaxTreeString("Method1(arg1).Method2(arg1)");
            AssertSyntaxTreeString("Method1(arg1).Prop1.Method2(arg1)");
            AssertSyntaxTreeString("Method1(arg1,arg2,arg3).Prop1.Method2(arg1,arg2,arg3).Prop2");
            AssertSyntaxTreeString("Prop1");
            AssertSyntaxTreeString("Prop1.Prop2");
            AssertSyntaxTreeString("Prop1.Method1(arg1,arg2,arg3).Prop2");
            AssertSyntaxTreeString("Prop1.Method1(arg1,arg2,arg3).Prop2.Method2(arg1,arg2,arg3)");

            AssertSyntaxTreeString("Method1(Method2())");
            AssertSyntaxTreeString("Method1(Method2(Method3(),Prop1),Prop2.Method4(),Prop3,1+2)");

            AssertSyntaxTreeString("(1+2).GetType()");
            AssertArithmetic("(1+2).GetType()", typeof(int));
        }
        [Test]
        public virtual void PropertyAndMethod() {
            AssertArithmetic("a + a", 2, "a", new[] { 1 });
            AssertArithmetic("a + b", 2, "a;b", new[] { 1, 1 });
            AssertArithmetic("a.b + a.b", 2, "a.b", new[] { 1 });
            AssertArithmetic("a.b + a.b.c", 2, "a.b;a.b.c", new[] { 1, 1 });
            AssertArithmetic("a.b.c + a.b.d", 2, "a.b.c;a.b.d", new[] { 1, 1 });
            AssertArithmetic("a.GetSelf(a.GetSelf()).IntProp", 1, "a", new[] { new BaseParserTests_a(intV: 1) });

            AssertArithmetic("a.GetSelf().IntProp", 1, "a", new[] { new BaseParserTests_a(intV: 1) });
            AssertArithmetic("a.GetSelf().IntField", 1, "a", new[] { new BaseParserTests_a(intV: 1) });
            AssertArithmetic("a.GetSelf().IntProp + a.GetSelf().IntField", 2, "a", new[] { new BaseParserTests_a(intV: 1) });
            AssertArithmetic("a.GetSelf().IntProp + a.GetSelf().DoubleField", 2.1, "a", new[] { new BaseParserTests_a(intV: 1, doubleV: 1.1) });
            AssertArithmetic("a.GetSelf().StringProp + a.GetSelf().StringField", "aa", "a", new[] { new BaseParserTests_a(stringV: "a") });
            AssertArithmetic("a.GetSelf().Self.GetSelf().IntProp", 1, "a", new[] { new BaseParserTests_a(intV: 1) });

            AssertArithmetic("a.GetInt(1)", 1, "a", new[] { new BaseParserTests_a() });
            AssertArithmetic("a.GetInt(1.1)", 1, "a", new[] { new BaseParserTests_a() });
            AssertArithmetic("a.GetObject(1)", 1, "a", new[] { new BaseParserTests_a() });
            AssertArithmetic("a.GetObject(1d)", 1d, "a", new[] { new BaseParserTests_a() });
            AssertArithmetic("a.GetObject(`a`)", "a", "a", new[] { new BaseParserTests_a() });
        }
        [Test]
        public virtual void Type() {
            AssertSyntaxTreeString("$Type.Prop", "Type.Prop");
            AssertSyntaxTreeString("Prop.Prop");
            AssertSyntaxTreeString("$xx:Type.Prop", "xx:Type.Prop");
            AssertSyntaxTreeString("$xx:Type.Prop==null?$xx:Type.Prop:$xx:Type.Prop", "xx:Type.Prop==null?xx:Type.Prop:xx:Type.Prop");
            AssertSyntaxTreeString("$Type.Prop1.Prop2.Prop3", "Type.Prop1.Prop2.Prop3");
            AssertSyntaxTreeString("$Type.StaticSelf.IntProp", "Type.StaticSelf.IntProp");
            AssertSyntaxTreeString("$Type.Method()", "Type.Method()");
            AssertSyntaxTreeString("($Type)a", "(Type)a");
            AssertSyntaxTreeString("($x:a)($b)($x:b)a", "(x:a)(b)(x:b)a");

            Func<string, Type> typeResolver = x => {
                if(x == "Type" || x == "x:Type") return typeof(BaseParserTests_a);
                throw new InvalidOperationException();
            };
            BaseParserTests_a.Static(1);
            AssertArithmetic("$Type.StaticIntProp", 1, null, null, typeResolver);
            AssertArithmetic("$Type.StaticIntProp + $Type.StaticIntField", 2, null, null, typeResolver);
            AssertArithmetic("$Type.StaticSelf.IntProp", 1, null, null, typeResolver);
            AssertArithmetic("$Type.StaticSelf.IntProp + $Type.StaticGetInt()", 2, null, null, typeResolver);

            AssertArithmetic("$x:Type.StaticIntProp", 1, null, null, typeResolver);
            AssertArithmetic("$x:Type.StaticIntProp + $x:Type.StaticIntField", 2, null, null, typeResolver);
            AssertArithmetic("$x:Type.StaticSelf.IntProp", 1, null, null, typeResolver);
            AssertArithmetic("$x:Type.StaticSelf.IntProp + $x:Type.StaticGetInt()", 2, null, null, typeResolver);

            AssertSyntaxTreeString("int.MaxValue", "int.MaxValue");
            AssertArithmetic("int.MaxValue", int.MaxValue);
        }
        [Test]
        public virtual void Attached() {
            AssertSyntaxTreeString("($Type.Attached1)", "(Type.Attached1)");
            AssertSyntaxTreeString("($Type.Attached1).Prop", "(Type.Attached1).Prop");
            AssertSyntaxTreeString("($Type.Attached1).Method()", "(Type.Attached1).Method()");
            AssertSyntaxTreeString("Prop.($Type.Attached1)", "Prop.(Type.Attached1)");
            AssertSyntaxTreeString("Method().($Type.Attached1).Prop", "Method().(Type.Attached1).Prop");
            AssertSyntaxTreeString("Prop.($Type.Attached1).($Type.Attached2).Method()", "Prop.(Type.Attached1).(Type.Attached2).Method()");
        }
        [Test, Category("T476018")]
        public virtual void Attached_NotAttached() {
            AssertSyntaxTreeString("($sys:DateTime.Now)", "(sys:DateTime.Now)");
            AssertSyntaxTreeString("($DateTime.Now)", "(DateTime.Now)");
            AssertSyntaxTreeString("($sys:DateTime.Now.Year)", "sys:DateTime.Now.Year");
            AssertSyntaxTreeString("($DateTime.Now.Year)", "DateTime.Now.Year");
            AssertSyntaxTreeString("($sys:DateTime.Now())", "sys:DateTime.Now()");
            AssertSyntaxTreeString("($DateTime.Now())", "DateTime.Now()");
            AssertSyntaxTreeString("($sys:DateTime.Now().Year)", "sys:DateTime.Now().Year");
            AssertSyntaxTreeString("($DateTime.Now().Year)", "DateTime.Now().Year");
            AssertSyntaxTreeString("($sys:DateTime.Now.Year - Row.HireDate.Year)", "sys:DateTime.Now.Year-Row.HireDate.Year");
        }
        [Test]
        public virtual void TypeOf() {
            AssertSyntaxTreeString("typeof ( $ Window )", "typeof(Window)");
            AssertSyntaxTreeString("Method(typeof($Type))", "Method(typeof(Type))");
            AssertSyntaxTreeString("($Type.Prop1).Method1(typeof($Type))", "(Type.Prop1).Method1(typeof(Type))");

            AssertSyntaxTreeString("typeof ( $ dx : Window )", "typeof(dx:Window)");
            AssertSyntaxTreeString("Method(typeof($dx:Type))", "Method(typeof(dx:Type))");
            AssertSyntaxTreeString("($dx:Type.Prop1).Method1(typeof($dx:Type))", "(dx:Type.Prop1).Method1(typeof(dx:Type))");

            AssertSyntaxTreeString("typeof($dx:Window).GetMethod()", "typeof(dx:Window).GetMethod()");

            AssertArithmetic("typeof(int)", typeof(int));
        }
#if !DXCORE3
        [Test]
        [SetCulture("ru-RU")]
        public virtual void ConstantWithCulture() {
            Constant();
        }
#endif
        [Test]
        public virtual void Constant() {
            AssertConstant("4", NConstant.NKind.Integer, 4);
            AssertConstant("4U", NConstant.NKind.Integer, 4U);
            AssertConstant("4u", NConstant.NKind.Integer, 4u);
            AssertConstant("4L", NConstant.NKind.Integer, 4L);
            AssertConstant("4UL", NConstant.NKind.Integer, 4UL);
            AssertConstant("4Ul", NConstant.NKind.Integer, 4Ul);
            AssertConstant("4uL", NConstant.NKind.Integer, 4uL);
            AssertConstant("4ul", NConstant.NKind.Integer, 4ul);
            AssertConstant("4LU", NConstant.NKind.Integer, 4LU);
            AssertConstant("4Lu", NConstant.NKind.Integer, 4Lu);
            AssertConstant("0x1", NConstant.NKind.Integer, 0x1);
            AssertConstant("0X1", NConstant.NKind.Integer, 0X1);
            AssertConstant("0x1U", NConstant.NKind.Integer, 0x1U);

            AssertConstant(".1", NConstant.NKind.Float, .1);
            AssertConstant("1.1", NConstant.NKind.Float, 1.1);
            AssertConstant("1e1", NConstant.NKind.Float, 1e1);
            AssertConstant("1e+1", NConstant.NKind.Float, 1e+1);
            AssertConstant("1e-1", NConstant.NKind.Float, 1e-1);
            AssertConstant(".1e1", NConstant.NKind.Float, .1e1);
            AssertConstant(".1e+1", NConstant.NKind.Float, .1e+1);
            AssertConstant(".1e-1", NConstant.NKind.Float, .1e-1);

            AssertConstant("1F", NConstant.NKind.Float, 1F);
            AssertConstant("1f", NConstant.NKind.Float, 1f);
            AssertConstant("1D", NConstant.NKind.Float, 1D);
            AssertConstant("1d", NConstant.NKind.Float, 1d);
            AssertConstant("1M", NConstant.NKind.Float, 1M);
            AssertConstant("1m", NConstant.NKind.Float, 1m);
            AssertConstant(".1e-1F", NConstant.NKind.Float, .1e-1F);

            AssertConstant("true", NConstant.NKind.Boolean, true);
            AssertConstant("false", NConstant.NKind.Boolean, false);

            AssertConstant("null", NConstant.NKind.Null, null);

            AssertConstant("`test`", NConstant.NKind.String, "test", false);
            AssertSyntaxTreeString("`test`", "\"test\"");
        }
        [Test]
        public virtual void Unary() {
            AssertSyntaxTreeString("-1");
            AssertSyntaxTreeString("+1", "1");
            AssertSyntaxTreeString("--+1", "--1");
            AssertSyntaxTreeString("++1", "1");
            AssertArithmetic("!false", true); AssertArithmetic("!a", true, "a", new[] { false });
            AssertArithmetic("~false", true); AssertArithmetic("~a", true, "a", new[] { false });
            AssertArithmetic("(a) + 1", 2, "a", new[] { 1 });
        }
        [Test]
        public virtual void Cast() {
            AssertSyntaxTreeString("(sbyte)(1)", "(sbyte)1");
            AssertArithmetic("(sbyte)(2.2+1)", (sbyte)3);
            AssertArithmetic("( sbyte)( 2.2+ 1)", (sbyte)3);
            AssertArithmetic("(  sbyte) ( byte ) (2.1)", (sbyte)2);
            AssertArithmetic("- - - + ( int) (byte ) (2.1)", - - -+(int)(byte)(2.1));
            AssertArithmetic("- - - + ( long) (int ) -2.1", - - -+(long)(int)-2);
            AssertSyntaxTreeString("($Type)1", "(Type)1");
            AssertSyntaxTreeString("($x:Type)1", "(x:Type)1");

            AssertSyntaxTreeString("1 is int", "(1 is int)");
            AssertSyntaxTreeString("1 as int", "(1 as int)");
            AssertArithmetic("1 is int", true);
            AssertArithmetic("1 is double", false);
            AssertArithmetic("1d is double", true);
            AssertArithmetic("1d is double?", true);
            AssertArithmetic("1 is double?", false);
            AssertArithmetic("1 as double?", null);
            AssertArithmetic("1d as double?", 1d);
        }
        [Test]
        public virtual void CastInTernary() {
            AssertArithmetic("(double?)@c ?? 2", (double)2, null, new object[] { null }, null);
            AssertArithmetic("(double?)@c ?? 2", (double)1, null, new object[] { 1 }, null);
        }
        [Test]
        public virtual void Relative() {
            AssertSyntaxTreeString("@DataContext", "");
            AssertSyntaxTreeString("@Self");
            AssertSyntaxTreeString("@TemplatedParent");
            AssertSyntaxTreeString("@ElementName(ElName)");
            AssertSyntaxTreeString("@StaticResource(ResName)");
            AssertSyntaxTreeString("@FindAncestor($x:Window,1)", "@FindAncestor(x:Window,1)");

            AssertSyntaxTreeString("@c", "");
            AssertSyntaxTreeString("@c.Prop", ".Prop");
            AssertSyntaxTreeString("@c.Method()", ".Method()");

            AssertSyntaxTreeString("@s");
            AssertSyntaxTreeString("@s.Prop");
            AssertSyntaxTreeString("@s.Method()");

            AssertSyntaxTreeString("@p");
            AssertSyntaxTreeString("@p.Prop");
            AssertSyntaxTreeString("@p.Method()");

            AssertSyntaxTreeString("@e(ElementName)");
            AssertSyntaxTreeString("@e(ElementName).Prop");
            AssertSyntaxTreeString("@e(ElementName).Method()");

            AssertSyntaxTreeString("@r(ResourceName)");
            AssertSyntaxTreeString("@r(ResourceName).Prop");
            AssertSyntaxTreeString("@r(ResourceName).Method()");

            AssertSyntaxTreeString("@a($Window)", "@a(Window)");
            AssertSyntaxTreeString("@a($Window).Prop", "@a(Window).Prop");
            AssertSyntaxTreeString("@a($Window).Method()", "@a(Window).Method()");

            AssertSyntaxTreeString("@a($x:Window)", "@a(x:Window)");
            AssertSyntaxTreeString("@a($x:Window).Prop", "@a(x:Window).Prop");
            AssertSyntaxTreeString("@a($x:Window).Method()", "@a(x:Window).Method()");

            AssertSyntaxTreeString("@a($Window,2)", "@a(Window,2)");
            AssertSyntaxTreeString("@a($Window,2).Prop", "@a(Window,2).Prop");
            AssertSyntaxTreeString("@a($Window,2).Method()", "@a(Window,2).Method()");

            AssertSyntaxTreeString("Method(@s.Prop,@a($x:Window,2))", "Method(@s.Prop,@a(x:Window,2))");

            AssertArithmetic("@s", 1, "", new[] { 1 });
            AssertArithmetic("@s.Prop", 1, "Prop", new[] { 1 });
            AssertArithmetic("@c", 1, "", new[] { 1 });
            AssertArithmetic("@c.Prop", 1, "Prop", new[] { 1 });

            AssertSyntaxTreeString("$int.Parse(@s.Text)", "int.Parse(@s.Text)");
            AssertArithmetic("$int.Parse(@s.Text)", 1, null, new[] { "1" });
        }
        [Test]
        public virtual void Index() {
            AssertSyntaxTreeString("[1]");
            AssertSyntaxTreeString("[1][2]", "[1].[2]");
            AssertSyntaxTreeString("a[1].A", "a.[1].A");
            AssertSyntaxTreeString("a[1,1][2].A", "a.[1,1].[2].A");
            AssertSyntaxTreeString("a()[1].A", "a().[1].A");
            AssertSyntaxTreeString("a(a[1], b[2])[1].A", "a(a.[1],b.[2]).[1].A");
            AssertSyntaxTreeString("a.b[1]", "a.b.[1]");
            AssertSyntaxTreeString("$Type.Prop[1]", "Type.Prop.[1]");
            AssertSyntaxTreeString("($Type.Prop)[1]", "(Type.Prop).[1]");
            AssertSyntaxTreeString("(1+2)[1]", "(1+2).[1]");

            AssertOperands("a[1].A", "a");
            AssertOperands("a.GetSelf()[1].A", "a");

            AssertArithmetic("a.GetSelf().Array[1]", "b", null,
                new[] { new BaseParserTests_a(array: new string[] { "a", "b" }) });
        }
        [Test]
        public virtual void OperandsOrder() {
            AssertOperands("-a*b/(+c*-d)/e", "a;b;c;d;e");
            AssertOperands("a*b/c*d", "a;b;c;d");
            AssertOperands("a == b ? c && d ? e : f : g", "a;b;c;d;e;f;g");
        }
        [Test]
        public virtual void ArithmeticNumeric() {
            AssertArithmetic("-1", -1);
            AssertArithmetic("+1", 1);
            AssertArithmetic("--+1", 1);
            AssertArithmetic("++1", 1);
            AssertArithmetic("1+2", 3); AssertArithmetic("a+b", 3, "a;b", new[] { 1, 2 });
            AssertArithmetic("1+2*2", 5); AssertArithmetic("a+b*c", 5, "a;b;c", new[] { 1, 2, 2 });
            AssertArithmetic("(1+2)*2", 6); AssertArithmetic("(a+b)*c", 6, "a;b;c", new[] { 1, 2, 2 });
            AssertArithmetic("2*2/4*3", 3); AssertArithmetic("a*b/c*d", 3, "a;b;c;d", new[] { 2, 2, 4, 3 });
            AssertArithmetic("2.1*2", 4.2); AssertArithmetic("a*b", 4.2, "a;b", new[] { 2.1, 2 });
            AssertArithmetic("2*2.1", 4.2); AssertArithmetic("a*b", 4.2, "a;b", new[] { 2, 2.1 });
            AssertArithmetic("5/2", 2); AssertArithmetic("a/b", 2, "a;b", new[] { 5, 2 });
            AssertArithmetic("5/2d", 2.5); AssertArithmetic("a/2d", 2.5, "a", new[] { 5 });
            AssertArithmetic("-4*2/(+2*2)/2", -1); AssertArithmetic("-a*b/(+c*d)/e", -1, "a;b;c;d;e", new[] { 4, 2, 2, 2, 2 });
            AssertArithmetic("-4*2/(+2*-2)/2", 1); AssertArithmetic("-a*b/(+c*d)/e", 1, "a;b;c;d;e", new[] { 4, 2, 2, -2, 2 });
            AssertArithmetic("--+4", 4); AssertArithmetic("--+a", 4, "a", new[] { 4 });
            AssertArithmetic("10>>2", 10 >> 2); AssertArithmetic("a>>b", 10 >> 2, "a;b", new[] { 10, 2 });
            AssertArithmetic("10<<2", 10 << 2); AssertArithmetic("a<<b", 10 << 2, "a;b", new[] { 10, 2 });
            AssertArithmetic("10<<-2", 10 << -2); AssertArithmetic("a<<-b", 10 << -2, "a;b", new[] { 10, 2 });

            AssertArithmetic("3&2", 3 & 2);
            AssertArithmetic("3|2", 3 | 2);
            AssertArithmetic("3^2", 3 ^ 2);
            AssertArithmetic("1&2|3^4", 1 & 2 | 3 ^ 4);
            AssertArithmetic("true&true^true", true & true ^ true);
            AssertArithmetic("true&false^true", true & false ^ true);
            AssertArithmetic("true&false|true", true & false | true);

            AssertArithmetic("`a`+`b`", "ab"); AssertArithmetic("`a`+b", "ab", "b", new[] { "b" });
            AssertArithmetic("1+1+2", 4); AssertArithmetic("a+a+b", 4, "a;b", new[] { 1, 2 });
        }
        [Test]
        public virtual void ArithmeticEquality() {
            AssertArithmetic("2 > 1.1", 2 > 1.1);
            AssertArithmetic("1.1 > 2", 1.1 > 2);
            AssertArithmetic("2d > 2", 2d > 2);
            AssertArithmetic("2d < 2", 2d < 2);
            AssertArithmetic("2d >= 2", 2d >= 2);
            AssertArithmetic("2d <= 2", 2d <= 2);

            AssertArithmetic("1 == 2", 1 == 2);
            AssertArithmetic("1 != 2", 1 != 2);
            AssertArithmetic("1.1 == 2", 1.1 == 2);
            AssertArithmetic("1 != 2.1", 1 != 2.1);
            AssertArithmetic("1 == 1", 1 == 1);
            AssertArithmetic("1d == 1", 1d == 1);
            AssertArithmetic("1d == 1d", 1d == 1d);
            AssertArithmetic("1 != 1", 1 != 1);
            AssertArithmetic("1d != 1", 1d != 1);
            AssertArithmetic("1d != 1d", 1d != 1d);
        }
        [Test]
        public virtual void ArithmeticLogical() {
            AssertArithmetic("(true || true) && false", (true || true) && false);
            AssertArithmetic("true || (true && false)", true || (true && false));
            AssertArithmetic("true || true && false", true || true && false);
            AssertArithmetic("true == false", true == false);
            AssertArithmetic("true != false", true != false);

            AssertArithmetic("!true", !true);
            AssertArithmetic("!false", !false);
            AssertArithmetic("~4", ~4);

            AssertSyntaxTreeString("4??5");
            AssertArithmetic("true ? true : false", true);
            AssertArithmetic("1 == 2 ? true : false", false);
            AssertArithmetic("1 == 2 ? true : 1 == 2", false);
            AssertArithmetic("1 == 1 ? true && true : false", true);
            AssertArithmetic("1 == 1 ? true && true ? 1 : 2 : 3", 1 == 1 ? true && true ? 1 : 2 : 3);
        }
        [Test, Category("T491236")]
        public virtual void TernaryOperations() {
            AssertSyntaxTreeString("@e(el1).Pr1?@e(el2).Pr2:@e(el3).Pr3");
            AssertArithmetic("@e(el1).BoolProp?@e(el2).IntProp:@e(el3).IntProp",
                2, "BoolProp;IntProp;IntProp", new object[] { true, 2, 3 });
            AssertArithmetic("@e(el1).BoolProp?@e(el2).IntProp:@e(el3).IntProp",
                3, "BoolProp;IntProp;IntProp", new object[] { false, 2, 3 });
        }

        [Test]
        public virtual void BackMode_Basic() {
            BackMode_AssertSyntaxTreeString("a=1");
            BackMode_AssertSyntaxTreeString("b=@v;; ; ", "b=@v");
            BackMode_AssertSyntaxTreeString("c=@value");
            BackMode_AssertSyntaxTreeString("a=1;b=2");
            BackMode_AssertSyntaxTreeString("a=1;;;b=2;;c=3;", "a=1;b=2;c=3");
            BackMode_AssertSyntaxTreeString("($dx:Type.Prop)=@v?@v.Prop+@value:@v", "(dx:Type.Prop)=@v?@v.Prop+@value:@v");

            BackMode_AssertSyntaxTreeString("1");
            BackMode_AssertSyntaxTreeString("@v");
            BackMode_AssertSyntaxTreeString("!@v");
            BackMode_AssertSyntaxTreeString("2+@v");
        }
        [Test]
        public virtual void BackMode_Operand() {
            IList<Operand> operands;
            operands = GetOperands("Prop", "@v").ToList();
            Assert.AreEqual(1, operands.Count);
            AssertOperand(operands[0], "Prop", true);

            operands = GetOperands("Prop", "Prop=@v").ToList();
            Assert.AreEqual(1, operands.Count);
            AssertOperand(operands[0], "Prop", true);

            operands = GetOperands("Prop1", "Prop2=@v").ToList();
            Assert.AreEqual(2, operands.Count);
            AssertOperand(operands[0], "Prop1", false);
            AssertOperand(operands[1], "Prop2", true);

            operands = GetOperands("Prop1+Prop2", "Prop3=@v").ToList();
            Assert.AreEqual(3, operands.Count);
            AssertOperand(operands[0], "Prop1", false);
            AssertOperand(operands[1], "Prop2", false);
            AssertOperand(operands[2], "Prop3", true);


            operands = GetOperands("Prop1+Prop2", "Prop2=@v;Prop3=@v").ToList();
            Assert.AreEqual(3, operands.Count);
            AssertOperand(operands[0], "Prop1", false);
            AssertOperand(operands[1], "Prop2", true);
            AssertOperand(operands[2], "Prop3", true);
        }

        [Test]
        public virtual void InvalidProperty() {
            AssertValidation("GetSelf().MethodA(1)", null,
                new object[] { null }, null);
            AssertValidation("GetSelf().MethodA(1)", 1,
                new object[] { new BaseValidationTests_a() }, null);
            AssertValidation("GetSelf().MethodA(1)", null,
                    new object[] { new BaseValidationTests_b() },
                    @"The 'MethodA(Int32)' method is not found on object 'BaseValidationTests_b'.");
        }
        [Test]
        public virtual void AcceptEscapedStringLiterals() {
            BackMode_AssertSyntaxTreeString("a = `\\``", "a=\"`\"");
            BackMode_AssertSyntaxTreeString("a = `a\\`\\n\\t' \\0 \\a \\b \\f \\r \\v`", "a=\"a`\n\t' \0 \a \b \f \r \v\"");
        }
        [Test, Category("T504788")]
        public virtual void MethodParameters() {
            AssertArithmetic(
                "$sys:String.Format(`Page: {0} of {1}`, IntProp, DoubleProp)",
                "Page: 1 of 2",
                null,
                new[] { 1, 2d },
                x => typeof(string));
        }
        [Test]
        public virtual void AttachedOperands() {
            AssertArithmetic(
                "($x:Class.AttachedProp)",
                1,
                null,
                new object[] { 1 },
                null);
            AssertArithmetic(
                "$sys:Math.Max(($x:Class.AttachedProp1), ($x:Class.AttachedProp2))",
                2,
                null,
                new object[] { 1, 2 },
                x => typeof(Math));
            AssertArithmetic(
                "$sys:Math.Max(@e(element1).($x:Class.AttachedProp1), @e(element2).($x:Class.AttachedProp2))",
                2,
                null,
                new object[] { 1, 2 },
                x => typeof(Math));
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class BaseParserTests_CommandToMethod : BaseTestFixtureBase {
        [Test]
        public void CanExecuteBasicTest() {
            CanExecute_AssertSyntaxTreeString("true && false", "True&&False");
            CanExecute_AssertSyntaxTreeString("Prop1.CanMethod(@e(ElementName),@parameter.Something())");
        }
        [Test]
        public void ExecuteArithmetic() {
            var vm = new BaseParserTests_CommandToMethod_a();
            Execute_AssertArithmetic("Execute1(true)", new[] { vm }, null);
            Assert.AreEqual(true, vm.Execute1Res);
            vm.Tag = false;
            Execute_AssertArithmetic("Execute1(Tag)", new[] { vm, vm.Tag }, null);
            Assert.AreEqual(false, vm.Execute1Res);
        }
        #region Research How the Csearches for the appropriate method overload
        [Test]
        public void ResearchStandardMethodSearchTest1() {
            var obj = new ResearchStandardMethodSearch();
            obj.A(1, 1); Assert.IsTrue(obj.A3); obj.Clear();
            obj.A("1", 1); Assert.IsTrue(obj.A2); obj.Clear();
            obj.A("1", "1"); Assert.IsTrue(obj.A1); obj.Clear();
            obj.A(1, "1"); Assert.IsTrue(obj.A4); obj.Clear();

            obj.A(1, 1); Assert.IsTrue(obj.A3); obj.Clear();
            obj.A('1', 1); Assert.IsTrue(obj.A3); obj.Clear();
            obj.A('1', '1'); Assert.IsTrue(obj.A5); obj.Clear();
            obj.A(1, '1'); Assert.IsTrue(obj.A3); obj.Clear();
        }
        [Test]
        public void MethodSearchTest1() {
            var obj = new ResearchStandardMethodSearch();
            Action<string> ex = x => Execute_AssertArithmetic(x, new[] { obj }, null);
            ex("A(1, 1)"); Assert.IsTrue(obj.A3); obj.Clear();
            ex("A(`1`, 1)"); Assert.IsTrue(obj.A2); obj.Clear();
            ex("A(`1`, `1`)"); Assert.IsTrue(obj.A1); obj.Clear();
            ex("A(1, `1`)"); Assert.IsTrue(obj.A4); obj.Clear();
        }
        [Test]
        public void ResearchStandardMethodSearchTest2() {
            var obj = new ResearchStandardMethodSearch();
            string o = "1";
            Class_a1 a1 = new Class_a1();
            Class_a2 a2 = new Class_a2();
            Class_a3 a3 = new Class_a3();

            obj.B(o, o); Assert.IsTrue(obj.B1); obj.Clear();
            obj.B(o, a1); Assert.IsTrue(obj.B2); obj.Clear();
            obj.B(o, a2); Assert.IsTrue(obj.B3); obj.Clear();
            obj.B(o, a3); Assert.IsTrue(obj.B3); obj.Clear();

            obj.B(a1, o); Assert.IsTrue(obj.B5); obj.Clear();
            obj.B(a1, a1); Assert.IsTrue(obj.B6); obj.Clear();
            obj.B(a1, a2); Assert.IsTrue(obj.B7); obj.Clear();
            obj.B(a1, a3); Assert.IsTrue(obj.B8); obj.Clear();

            obj.B(a2, o); Assert.IsTrue(obj.B9); obj.Clear();
            obj.B(a2, a1); Assert.IsTrue(obj.B10); obj.Clear();
            obj.B(a2, a2); Assert.IsTrue(obj.B11); obj.Clear();
            obj.B(a2, a3); Assert.IsTrue(obj.B12); obj.Clear();

            obj.B(a3, o); Assert.IsTrue(obj.B13); obj.Clear();
            obj.B(a3, a1); Assert.IsTrue(obj.B14); obj.Clear();
            obj.B(a3, a2); Assert.IsTrue(obj.B15); obj.Clear();
        }
        [Test]
        public virtual void MethodSearchTest2() {
            var obj = new ResearchStandardMethodSearch();
            string o = "1";
            Class_a1 a1 = new Class_a1();
            Class_a2 a2 = new Class_a2();
            Class_a3 a3 = new Class_a3();
            Class_a4 a4 = new Class_a4();

            Action<string> ex = x => Execute_AssertArithmetic("o; a1; a2; a3; a4; " + x, new object[] { o, a1, a2, a3, a4, obj }, null);

            ex("B(o, o)"); Assert.IsTrue(obj.B1); obj.Clear();
            ex("B(o, a1)"); Assert.IsTrue(obj.B2); obj.Clear();
            ex("B(o, a2)"); Assert.IsTrue(obj.B3); obj.Clear();
            ex("B(o, a3)"); Assert.IsTrue(obj.B3); obj.Clear();

            ex("B(a1, o)"); Assert.IsTrue(obj.B5); obj.Clear();
            ex("B(a1, a1)"); Assert.IsTrue(obj.B6); obj.Clear();
            ex("B(a1, a2)"); Assert.IsTrue(obj.B7); obj.Clear();
            ex("B(a1, a3)"); Assert.IsTrue(obj.B8); obj.Clear();

            ex("B(a2, o)"); Assert.IsTrue(obj.B9); obj.Clear();
            ex("B(a2, a1)"); Assert.IsTrue(obj.B10); obj.Clear();
            ex("B(a2, a2)"); Assert.IsTrue(obj.B11); obj.Clear();
            ex("B(a2, a3)"); Assert.IsTrue(obj.B12); obj.Clear();

            ex("B(a3, o)"); Assert.IsTrue(obj.B13); obj.Clear();
            ex("B(a3, a1)"); Assert.IsTrue(obj.B14); obj.Clear();
            ex("B(a3, a2)"); Assert.IsTrue(obj.B15); obj.Clear();
            Execute_AssertValidation("o; a1; a2; a3; a4; " + "B(a3, a3)",
                new object[] { o, a1, a2, a3, a4, obj }, "The 'B(Class_a3, Class_a3)' method is not found on object 'ResearchStandardMethodSearch'.");
            Execute_AssertValidation("o; a1; a2; a3; a4; " + "C(a4, a4)",
                new object[] { o, a1, a2, a3, a4, obj }, "The 'C(Class_a4, Class_a4)' method is not found on object 'ResearchStandardMethodSearch'.");
        }
        protected class ResearchStandardMethodSearch {
            #region A
            public bool A1 { get; set; }
            public bool A2 { get; set; }
            public bool A3 { get; set; }
            public bool A4 { get; set; }
            public bool A5 { get; set; }
            public void A(object p1, object p2) { A1 = true; }
            public void A(object p1, int p2) { A2 = true; }
            public void A(int p1, int p2) { A3 = true; }
            public void A(int p1, object p2) { A4 = true; }
            public void A(char p1, char p2) { A5 = true; }
            #endregion
            #region B
            public bool B1 { get; set; }
            public bool B2 { get; set; }
            public bool B3 { get; set; }
            public bool B4 { get; set; }
            public bool B5 { get; set; }
            public bool B6 { get; set; }
            public bool B7 { get; set; }
            public bool B8 { get; set; }
            public bool B9 { get; set; }
            public bool B10 { get; set; }
            public bool B11 { get; set; }
            public bool B12 { get; set; }
            public bool B13 { get; set; }
            public bool B14 { get; set; }
            public bool B15 { get; set; }
            public bool B16 { get; set; }

            public void B(object p1, object p2) { B1 = true; }
            public void B(object p1, Class_a1 p2) { B2 = true; }
            public void B(object p1, Class_a2 p2) { B3 = true; }
            public void B(Class_a1 p1, object p2) { B5 = true; }
            public void B(Class_a1 p1, Class_a1 p2) { B6 = true; }
            public void B(Class_a1 p1, Class_a2 p2) { B7 = true; }
            public void B(Class_a1 p1, Class_a3 p2) { B8 = true; }
            public void B(Class_a2 p1, object p2) { B9 = true; }
            public void B(Class_a2 p1, Class_a1 p2) { B10 = true; }
            public void B(Class_a2 p1, Class_a2 p2) { B11 = true; }
            public void B(Class_a2 p1, Class_a3 p2) { B12 = true; }
            public void B(Class_a3 p1, object p2) { B13 = true; }
            public void B(Class_a3 p1, Class_a1 p2) { B14 = true; }
            public void B(Class_a3 p1, Class_a2 p2) { B15 = true; }
            #endregion
            #region C
            public void C(Class_a2 p1, Class_a1 p2) { }
            public void C(Class_a1 p1, Class_a3 p2) { }
            #endregion

            public void Clear() {
                A1 = A2 = A3 = A4 = A5 = false;
                B1 = B2 = B3 = B4 = B5 = B6 = B7 = B8 = B9 = B10 = B11 = B12 = B13 = B14 = B15 = B16 = false;
            }
        }
        public class Class_a1 { }
        public class Class_a2 : Class_a1 { }
        public class Class_a3 : Class_a2 { }
        public class Class_a4 : Class_a3 { }
        #endregion
    }

    [Platform("NET")]
    [TestFixture]
    public class BaseParserTests_Dynamics : BaseParserTests {
        internal override IBindingCalculator CreateBindingCalculator(BindingTreeInfoTest info, TestErrorHandler errorHandler) {
            return new BindingCalculatorDynamic(info.Expr, info.BackExpr, null, errorHandler);
        }
        internal override ICommandCalculator CreateCommandCalculator(CommandTreeInfo info, TestErrorHandler errorHandler) {
            return new CommandCalculatorDynamic(info.ExecuteExpr, info.CanExecuteExpr, true, errorHandler);
        }
        [Test]
        public override void CastInTernary() {
            AssertArithmetic("(double?)@c ?? 2", (int)2, null, new object[] { null }, null);
            AssertArithmetic("(double?)@c ?? 2", (double?)1, null, new object[] { 1 }, null);
        }
        [Test]
        public override void Unary() {
            AssertSyntaxTreeString("-1");
            AssertSyntaxTreeString("+1", "1");
            AssertSyntaxTreeString("--+1", "--1");
            AssertSyntaxTreeString("++1", "1");
            AssertArithmetic("!false", true); AssertArithmetic("!a", true, "a", new[] { false });
            AssertArithmetic("(a) + 1", 2, "a", new[] { 1 });
        }
        [Test]
        public void MemberSearcher_T495631() {
            AssertArithmetic("$string.IsNullOrEmpty(null)", true, null, null);
        }

        [Test]
        public void NewOperator_SyntaxTree() {
            AssertSyntaxTreeString("new int()");
            AssertSyntaxTreeString("new int?()");
            AssertSyntaxTreeString("new $dx:DXType()");
            AssertSyntaxTreeString("new $dx:DXType?()");

            AssertSyntaxTreeString("new int(1)");
            AssertSyntaxTreeString("new int?(2,3)");
            AssertSyntaxTreeString("new $dx:DXType(1)");
            AssertSyntaxTreeString("new $dx:DXType?(2,3)");

            AssertSyntaxTreeString("new int(1).PropertyA.PropertyB");
            AssertSyntaxTreeString("new int?(2,3).PropertyA.PropertyB");
            AssertSyntaxTreeString("new $dx:DXType(1).PropertyA.PropertyB");
            AssertSyntaxTreeString("new $dx:DXType?(2,3).PropertyA.PropertyB");

            AssertSyntaxTreeString("new int(1).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new int?(2,3).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new $dx:DXType(1).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new $dx:DXType?(2,3).MethodA(1).MethodB(1,2)");

            AssertSyntaxTreeString("new int(1).MethodA(1).MethodB(1,2)+new int(1).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new int?(2,3).MethodA(1).MethodB(1,2)+new int?(2,3).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new $dx:DXType(1).MethodA(1).MethodB(1,2)+new $dx:DXType(1).MethodA(1).MethodB(1,2)");
            AssertSyntaxTreeString("new $dx:DXType?(2,3).MethodA(1).MethodB(1,2)+new $dx:DXType?(2,3).MethodA(1).MethodB(1,2)");

            AssertSyntaxTreeString("new $dx:DXType((2).Method())");
            AssertSyntaxTreeString("new int?(1+2,new int(new $dx:DXType?()),2).Method(new $dx:DXType((2).Method()))");
            AssertSyntaxTreeString("Method(new int?(1+2,new int(new $dx:DXType?()),2),null).PropertyA");

            AssertSyntaxTreeString("new int()??new double?()");
            AssertSyntaxTreeString("new int()?new double?():new $dx:DXType()");
        }
        [Test]
        public void NewOperator_Arithmetic() {
            AssertArithmetic("new int()",
                x => Assert.That(object.Equals(x, new int())));
            AssertArithmetic("new int?()",
                x => Assert.That(object.Equals(x, new int?())));
            AssertArithmetic("new $dx:DXType()",
                x => Assert.That(x is NewOperator_a),
                null, null, x => typeof(NewOperator_a));
            AssertArithmetic("new $dx:DXType(1)",
                x => Assert.That(((NewOperator_a)x).V1 == 1),
                null, null, x => typeof(NewOperator_a));
            AssertArithmetic("new $dx:DXType(1+1)",
                x => Assert.That(((NewOperator_a)x).V1 == 2),
                null, null, x => typeof(NewOperator_a));
            AssertArithmetic("new $dx:DXType(1+1, 2)",
                x => Assert.That(((NewOperator_a)x).V1 == 2 && ((NewOperator_a)x).V2 == 2),
                null, null, x => typeof(NewOperator_a));
        }

        [Test]
        public override void ArithmeticEquality() {
            AssertArithmetic("2 > 1.1", 2 > 1.1);
            AssertArithmetic("1.1 > 2", 1.1 > 2);
            AssertArithmetic("2d > 2", 2d > 2);
            AssertArithmetic("2d < 2", 2d < 2);
            AssertArithmetic("2d >= 2", 2d >= 2);
            AssertArithmetic("2d <= 2", 2d <= 2);

            AssertArithmetic("1 == 2", 1 == 2);
            AssertArithmetic("1 != 2", 1 != 2);
            AssertArithmetic("1.1 == 2", 1.1 == 2);
            AssertArithmetic("1 != 2.1", 1 != 2.1);
            AssertArithmetic("1 == 1", 1 == 1);
            AssertArithmetic("1d == 1", 1d == 1);
            AssertArithmetic("1d == 1d", 1d == 1d);
            AssertArithmetic("1 != 1", 1 != 1);
            AssertArithmetic("1d != 1", 1d != 1);
            AssertArithmetic("1d != 1d", 1d != 1d);
        }

        public class NewOperator_a {
            public int V1 { get; set; }
            public double V2 { get; set; }
            public NewOperator_a() { }
            public NewOperator_a(int v, double v2 = 0) {
                V1 = v;
                V2 = v2;
            }
        }
    }

    [Platform("NET")]
    public class BaseParserTests_Dynamics_CommandToMethod : BaseParserTests_CommandToMethod {
        internal override IBindingCalculator CreateBindingCalculator(BindingTreeInfoTest info, TestErrorHandler errorHandler) {
            return new BindingCalculatorDynamic(info.Expr, info.BackExpr, null, errorHandler);
        }
        internal override ICommandCalculator CreateCommandCalculator(CommandTreeInfo info, TestErrorHandler errorHandler) {
            return new CommandCalculatorDynamic(info.ExecuteExpr, info.CanExecuteExpr, true, errorHandler);
        }

        [Test]
        public void ExecuteBasicTest() {
            Execute_AssertSyntaxTreeString("Prop1");
            Execute_AssertSyntaxTreeString("Prop1;; ; ", "Prop1");
            Execute_AssertSyntaxTreeString("Method()");
            Execute_AssertSyntaxTreeString("Method(Prop1)");
            Execute_AssertSyntaxTreeString("@s.Method(Prop1, @a($x:Type))", "@s.Method(Prop1,@a(x:Type))");
            Execute_AssertSyntaxTreeString("[@s.Get()].Method()");
            Execute_AssertSyntaxTreeString("Method1();Method2();Prop");
            Execute_AssertSyntaxTreeString("Method1(@parameter)");

            Execute_AssertArithmetic("@parameter", null, null, null);
            Execute_AssertArithmetic("1", null, null, null);
            Execute_AssertSyntaxTreeString("Method(1)");
        }

        protected class ResearchStandardMethodSearch2 : ResearchStandardMethodSearch {
            public string o = "1";
            public Class_a1 A1V = new Class_a1();
            public Class_a2 A2V = new Class_a2();
            public Class_a3 A3V = new Class_a3();
            public Class_a4 A4V = new Class_a4();
        }
        [Test]
        public override void MethodSearchTest2() {
            var obj = new ResearchStandardMethodSearch2();

            Action<string> ex = x => Execute_AssertArithmetic("o; A1V; A2V; A3V; A4V; " + x, new object[] { obj }, null);

            ex("B(o, o)"); Assert.IsTrue(obj.B1); obj.Clear();
            ex("B(o, A1V)"); Assert.IsTrue(obj.B2); obj.Clear();
            ex("B(o, A2V)"); Assert.IsTrue(obj.B3); obj.Clear();
            ex("B(o, A3V)"); Assert.IsTrue(obj.B3); obj.Clear();

            ex("B(A1V, o)"); Assert.IsTrue(obj.B5); obj.Clear();
            ex("B(A1V, A1V)"); Assert.IsTrue(obj.B6); obj.Clear();
            ex("B(A1V, A2V)"); Assert.IsTrue(obj.B7); obj.Clear();
            ex("B(A1V, A3V)"); Assert.IsTrue(obj.B8); obj.Clear();

            ex("B(A2V, o)"); Assert.IsTrue(obj.B9); obj.Clear();
            ex("B(A2V, A1V)"); Assert.IsTrue(obj.B10); obj.Clear();
            ex("B(A2V, A2V)"); Assert.IsTrue(obj.B11); obj.Clear();
            ex("B(A2V, A3V)"); Assert.IsTrue(obj.B12); obj.Clear();

            ex("B(A3V, o)"); Assert.IsTrue(obj.B13); obj.Clear();
            ex("B(A3V, A1V)"); Assert.IsTrue(obj.B14); obj.Clear();
            ex("B(A3V, A2V)"); Assert.IsTrue(obj.B15); obj.Clear();
            Execute_AssertValidation("o; A1V; A2V; A3V; A4V; " + "B(A3V, A3V)",
                new object[] { obj }, "The 'B(Class_a3, Class_a3)' method is not found on object 'ResearchStandardMethodSearch2'.");
            Execute_AssertValidation("o; A1V; A2V; A3V; A4V; " + "C(A4V, A4V)",
                new object[] { obj }, "The 'C(Class_a4, Class_a4)' method is not found on object 'ResearchStandardMethodSearch2'.");
        }
    }

    public class BaseParserTests_a {
        public static int StaticIntProp { get; set; }
        public static int StaticIntField { get; set; }
        public static BaseParserTests_a StaticSelf { get; set; }
        public static void Static(int intV = 0) {
            StaticIntProp = StaticIntField = intV;
            StaticSelf = new BaseParserTests_a().CreateInstance();
            StaticSelf.IntProp = intV;
        }
        public static int StaticGetInt() {
            return StaticIntProp;
        }

        public int @AtProp { get; set; }
        public int @int { get; set; }
        public virtual int IntProp { get; set; }
        public int IntField;
        public virtual double DoubleProp { get; set; }
        public double DoubleField;
        public virtual string StringProp { get; set; }
        public string StringField;
        public string[] Array { get; set; }
        public Dictionary<Type, string> Dictionary { get; set; }
        public BaseParserTests_a Self { get { return this; } }
        public BaseParserTests_a(int intV = 0, double doubleV = 0, string stringV = "", string[] array = null, Dictionary<Type, string> dictionary = null) {
            AtProp = @int = IntProp = IntField = intV;
            DoubleProp = DoubleField = doubleV;
            StringProp = StringField = stringV;
            Array = array;
            Dictionary = dictionary;
        }
        public BaseParserTests_a GetSelf() {
            return this;
        }
        public BaseParserTests_a GetSelf(BaseParserTests_a a) {
            return a;
        }
        public int GetInt(int v) {
            return v;
        }
        public int GetInt(double v) {
            return (int)v;
        }
        public object GetObject(int v) {
            return v;
        }
        public object GetObject(double v) {
            return v;
        }
        public object GetObject(object v) {
            return v;
        }

        public T Generic1<T>(T v) {
            return v;
        }

        protected virtual BaseParserTests_a CreateInstance() {
            return new BaseParserTests_a();
        }
    }
    public class BaseValidationTests_a {
        public BaseValidationTests_a GetSelf() { return this; }
        public int MethodA(int v) { return v; }
    }
    public class BaseValidationTests_b {
        public BaseValidationTests_b GetSelf() { return this; }
        public int MethodB(int v) { return v; }
    }
    public class BaseParserTests_CommandToMethod_a {
        public object Tag { get; set; }
        public object Execute1Res { get; set; }
        public object Execute2Res { get; set; }
        public void Execute1(object res) {
            Execute1Res = res;
        }
    }
}