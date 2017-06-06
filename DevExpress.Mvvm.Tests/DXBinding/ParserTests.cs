using DevExpress.DXBinding.Native;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.DXBinding.Tests {
    class ParserTestHelper {
        class BindingTreeInfoTest : BindingTreeInfo {
            public BindingTreeInfoTest(string input) : this(input, null) { }
            public BindingTreeInfoTest(string input, string backInput)
                : base(input, backInput, null) { }
            public string ExprToString() {
                return new VisitorString().Resolve(Expr);
            }
            public string BackExprToString() {
                return new VisitorString().Resolve(BackExpr);
            }
            protected override void Throw(string msg) {
                Assert.Fail(msg);
            }
        }
        class CommandTreeInfoTest : CommandTreeInfo {
            public CommandTreeInfoTest(string input, string canInput) : base(input, canInput, null) { }
            public string ExecuteExprToString() {
                return new VisitorString().Resolve(ExecuteExpr);
            }
            public string CanExecuteExprToString() {
                return new VisitorString().Resolve(CanExecuteExpr);
            }
            protected override void Throw(string msg) {
                Assert.Fail(msg);
            }
        }
        class TestTypeResolver : ITypeResolver {
            readonly Func<string, Type> func;
            public TestTypeResolver(Func<string, Type> func) {
                this.func = func;
            }
            Type ITypeResolver.ResolveType(string type) {
                return func(type);
            }
        }
        class TestErrorHandler : ErrorHandlerBase {
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

        public static void AssertSyntaxTreeString(string input, string expected = null) {
            AssertSyntaxTreeString(input, null, expected);
        }
        public static void BackMode_AssertSyntaxTreeString(string input, string expected = null) {
            AssertSyntaxTreeString(null, input, expected);
        }
        public static void Execute_AssertSyntaxTreeString(string input, string expected = null) {
            var info = new CommandTreeInfoTest(input, null);
            Assert.AreEqual(expected ?? input, info.ExecuteExprToString());
        }
        public static void CanExecute_AssertSyntaxTreeString(string input, string expected = null) {
            var info = new CommandTreeInfoTest(null, input);
            Assert.AreEqual(expected ?? input, info.CanExecuteExprToString());
        }
        static void AssertSyntaxTreeString(string input, string backInput, string expected) {
            var info = new BindingTreeInfoTest(input, backInput);
            if(input != null)
                Assert.AreEqual(expected ?? input, info.ExprToString());
            if(backInput != null)
                Assert.AreEqual(expected ?? backInput, info.BackExprToString());
        }

        public static void AssertIdent(string input, string expectedValue = null) {
            var info = new BindingTreeInfoTest(input);
            var n = (NIdent)info.Expr.Expr;
            Assert.AreEqual(expectedValue ?? input, info.ExprToString());
        }
        public static void AssertConstant(string input, NConstant.NKind expectedType, object expectedValue, bool checkSyntaxTreeString = true) {
            var info = new BindingTreeInfoTest(input);
            var n = (NConstant)info.Expr.Expr;
            Assert.AreEqual(expectedType, n.Kind);
            Assert.AreEqual(expectedValue, n.Value);
            if(expectedValue != null) Assert.AreEqual(expectedValue.GetType(), n.Value.GetType());
            if(checkSyntaxTreeString && expectedValue != null)
                AssertSyntaxTreeString(input, expectedValue.ToString());
        }

        public static void AssertOperands(string input, string operandsOrder) {
            var info = new BindingTreeInfoTest(input);
            var operands = GetOperands(input, null).Select(x => x.Path);
            var expOperands = operandsOrder == null ? new string[] { } : operandsOrder.Split(';');
            Assert.AreEqual(expOperands.Count(), operands.Count());
            for(int i = 0; i < operands.Count(); i++)
                Assert.AreEqual(expOperands.ElementAt(i), operands.ElementAt(i));
        }
        public static void AssertOperand(Operand operand, string expectedPath, bool shouldBeTwoWay) {
            Assert.AreEqual(expectedPath, operand.Path);
            Assert.AreEqual(shouldBeTwoWay, operand.IsTwoWay);
        }
        public static IEnumerable<Operand> GetOperands(string input, string backInput) {
            var info = new BindingTreeInfoTest(input, backInput);
            var visitor = new VisitorOperand();
            var errorHandler = new TestErrorHandler();
            var res = visitor.Resolve(new[] { info.Expr }, info.BackExpr, x => typeof(object), errorHandler);
            if(errorHandler.HasError) Assert.Fail();
            return res;
        }

        public static void AssertArithmetic(string input, object expected, string operandsOrder = null, Array operands = null, Func<string, Type> typeResolver = null) {
            if(operandsOrder != null) AssertOperands(input, operandsOrder);
            var info = new BindingTreeInfoTest(input);
            object[] ops = operands == null ? null : operands.Cast<object>().ToArray();
            BindingCalculator calculator = new BindingCalculator(info.Expr, info.BackExpr, null, new TestErrorHandler(true));
            calculator.Init(new TestTypeResolver(typeResolver));
            var res = calculator.Resolve(ops);
            Assert.AreEqual(expected, res);
            if(expected != null) Assert.AreEqual(expected.GetType(), res.GetType());
        }
        public static void Execute_AssertArithmetic(string input, object[] opValues, object parameter, Func<string, Type> typeResolver = null) {
            var info = new CommandTreeInfoTest(input, null);
            CommandCalculator calculator = new CommandCalculator(info.ExecuteExpr, info.CanExecuteExpr, true, new TestErrorHandler(true));
            calculator.Init(new TestTypeResolver(typeResolver));
            calculator.Execute(opValues, parameter);
        }

        public static void AssertValidation(string input, object expected, Array operands, string message, Func<string, Type> typeResolver = null) {
            var info = new BindingTreeInfoTest(input);
            var errorHandler = new TestErrorHandler();
            var calculator = new BindingCalculator(info.Expr, info.BackExpr, null, errorHandler);
            calculator.Init(new TestTypeResolver(typeResolver));
            var res = calculator.Resolve(operands.Cast<object>().ToArray());
            Assert.AreEqual(expected, res);
            if(expected != null) Assert.AreEqual(expected.GetType(), res.GetType());
            Assert.AreEqual(message, errorHandler.ErrorMsg);
        }
        public static void Execute_AssertValidation(string input, Array operands, string message, Func<string, Type> typeResolver = null) {
            var info = new CommandTreeInfoTest(input, null);
            var errorHandler = new TestErrorHandler();
            var calculator = new CommandCalculator(info.ExecuteExpr, info.CanExecuteExpr, true, errorHandler);
            calculator.Init(new TestTypeResolver(typeResolver));
            calculator.Execute(operands.Cast<object>().ToArray(), null);
            Assert.AreEqual(message, errorHandler.ErrorMsg);
        }
    }

    [TestFixture]
    public class ParserTests {
        [Test]
        public void Property() {
            ParserTestHelper.AssertIdent("a");
            ParserTestHelper.AssertIdent("a1");
            ParserTestHelper.AssertIdent("a.b");
            ParserTestHelper.AssertIdent("_._");
            ParserTestHelper.AssertIdent("_1._1");
            ParserTestHelper.AssertIdent("_1a._1a");
            ParserTestHelper.AssertSyntaxTreeString("-a");
            ParserTestHelper.AssertSyntaxTreeString("--+a.b", "--a.b");
            ParserTestHelper.AssertSyntaxTreeString("(sbyte1)+1", "sbyte1+1");
            ParserTestHelper.AssertSyntaxTreeString("`abc`.Length", "(\"abc\").Length");

            ParserTestHelper.AssertIdent("isa");
            ParserTestHelper.AssertIdent("bytes");

            ParserTestHelper.AssertArithmetic("a.GetSelf().AtProp", 1, null, new[] { new ParserTests_a(intV: 1) });
            ParserTestHelper.AssertArithmetic("a.GetSelf().int", 1, null, new[] { new ParserTests_a(intV: 1) });
        }
        [Test]
        public void Method() {
            ParserTestHelper.AssertSyntaxTreeString("Method1()");
            ParserTestHelper.AssertSyntaxTreeString("Method1().Method2()");
            ParserTestHelper.AssertSyntaxTreeString("Method1().Prop1.Method2()");
            ParserTestHelper.AssertSyntaxTreeString("Method1().Prop1.Method2().Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Method1().Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Method1().Prop2.Method2()");

            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1)");
            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1,arg2)");
            ParserTestHelper.AssertSyntaxTreeString("Method1(1+2 ,-arg2)", "Method1(1+2,-arg2)");

            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1)");
            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1).Method2(arg1)");
            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1).Prop1.Method2(arg1)");
            ParserTestHelper.AssertSyntaxTreeString("Method1(arg1,arg2,arg3).Prop1.Method2(arg1,arg2,arg3).Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Method1(arg1,arg2,arg3).Prop2");
            ParserTestHelper.AssertSyntaxTreeString("Prop1.Method1(arg1,arg2,arg3).Prop2.Method2(arg1,arg2,arg3)");

            ParserTestHelper.AssertSyntaxTreeString("Method1(Method2())");
            ParserTestHelper.AssertSyntaxTreeString("Method1(Method2(Method3(),Prop1),Prop2.Method4(),Prop3,1+2)");

            ParserTestHelper.AssertSyntaxTreeString("(1+2).GetType()");
            ParserTestHelper.AssertArithmetic("(1+2).GetType()", typeof(int));
        }
        [Test]
        public void PropertyAndMethod() {
            ParserTestHelper.AssertArithmetic("a + a", 2, "a", new[] { 1 });
            ParserTestHelper.AssertArithmetic("a + b", 2, "a;b", new[] { 1, 1 });
            ParserTestHelper.AssertArithmetic("a.b + a.b", 2, "a.b", new[] { 1 });
            ParserTestHelper.AssertArithmetic("a.b + a.b.c", 2, "a.b;a.b.c", new[] { 1, 1 });
            ParserTestHelper.AssertArithmetic("a.b.c + a.b.d", 2, "a.b.c;a.b.d", new[] { 1, 1 });
            ParserTestHelper.AssertArithmetic("a.GetSelf(a.GetSelf()).IntProp", 1, "a", new[] { new ParserTests_a(intV: 1) });

            ParserTestHelper.AssertArithmetic("a.GetSelf().IntProp", 1, "a", new[] { new ParserTests_a(intV: 1) });
            ParserTestHelper.AssertArithmetic("a.GetSelf().IntField", 1, "a", new[] { new ParserTests_a(intV: 1) });
            ParserTestHelper.AssertArithmetic("a.GetSelf().IntProp + a.GetSelf().IntField", 2, "a", new[] { new ParserTests_a(intV: 1) });
            ParserTestHelper.AssertArithmetic("a.GetSelf().IntProp + a.GetSelf().DoubleField", 2.1, "a", new[] { new ParserTests_a(intV: 1, doubleV: 1.1) });
            ParserTestHelper.AssertArithmetic("a.GetSelf().StringProp + a.GetSelf().StringField", "aa", "a", new[] { new ParserTests_a(stringV: "a") });
            ParserTestHelper.AssertArithmetic("a.GetSelf().Self.GetSelf().IntProp", 1, "a", new[] { new ParserTests_a(intV: 1) });

            ParserTestHelper.AssertArithmetic("a.GetInt(1)", 1, "a", new[] { new ParserTests_a() });
            ParserTestHelper.AssertArithmetic("a.GetInt(1.1)", 1, "a", new[] { new ParserTests_a() });
            ParserTestHelper.AssertArithmetic("a.GetObject(1)", 1, "a", new[] { new ParserTests_a() });
            ParserTestHelper.AssertArithmetic("a.GetObject(1d)", 1d, "a", new[] { new ParserTests_a() });
            ParserTestHelper.AssertArithmetic("a.GetObject(`a`)", "a", "a", new[] { new ParserTests_a() });
        }
        [Test]
        public void Type() {
            ParserTestHelper.AssertSyntaxTreeString("$Type.Prop", "Type.Prop");
            ParserTestHelper.AssertSyntaxTreeString("Prop.Prop");
            ParserTestHelper.AssertSyntaxTreeString("$xx:Type.Prop", "xx:Type.Prop");
            ParserTestHelper.AssertSyntaxTreeString("$xx:Type.Prop==null?$xx:Type.Prop:$xx:Type.Prop", "xx:Type.Prop==null?xx:Type.Prop:xx:Type.Prop");
            ParserTestHelper.AssertSyntaxTreeString("$Type.Prop1.Prop2.Prop3", "Type.Prop1.Prop2.Prop3");
            ParserTestHelper.AssertSyntaxTreeString("$Type.StaticSelf.IntProp", "Type.StaticSelf.IntProp");
            ParserTestHelper.AssertSyntaxTreeString("$Type.Method()", "Type.Method()");
            ParserTestHelper.AssertSyntaxTreeString("($Type)a", "(Type)a");
            ParserTestHelper.AssertSyntaxTreeString("($x:a)($b)($x:b)a", "(x:a)(b)(x:b)a");

            Func<string, Type> typeResolver = x => {
                if(x == "Type" || x == "x:Type") return typeof(ParserTests_a);
                throw new InvalidOperationException();
            };
            ParserTests_a.Static(1);
            ParserTestHelper.AssertArithmetic("$Type.StaticIntProp", 1, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$Type.StaticIntProp + $Type.StaticIntField", 2, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$Type.StaticSelf.IntProp", 1, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$Type.StaticSelf.IntProp + $Type.StaticGetInt()", 2, null, null, typeResolver);

            ParserTestHelper.AssertArithmetic("$x:Type.StaticIntProp", 1, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$x:Type.StaticIntProp + $x:Type.StaticIntField", 2, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$x:Type.StaticSelf.IntProp", 1, null, null, typeResolver);
            ParserTestHelper.AssertArithmetic("$x:Type.StaticSelf.IntProp + $x:Type.StaticGetInt()", 2, null, null, typeResolver);

            ParserTestHelper.AssertSyntaxTreeString("int.MaxValue", "int.MaxValue");
            ParserTestHelper.AssertArithmetic("int.MaxValue", int.MaxValue);
        }
        [Test]
        public void Attached() {
            ParserTestHelper.AssertSyntaxTreeString("($Type.Attached1)", "(Type.Attached1)");
            ParserTestHelper.AssertSyntaxTreeString("($Type.Attached1).Prop", "(Type.Attached1).Prop");
            ParserTestHelper.AssertSyntaxTreeString("($Type.Attached1).Method()", "(Type.Attached1).Method()");
            ParserTestHelper.AssertSyntaxTreeString("Prop.($Type.Attached1)", "Prop.(Type.Attached1)");
            ParserTestHelper.AssertSyntaxTreeString("Method().($Type.Attached1).Prop", "Method().(Type.Attached1).Prop");
            ParserTestHelper.AssertSyntaxTreeString("Prop.($Type.Attached1).($Type.Attached2).Method()", "Prop.(Type.Attached1).(Type.Attached2).Method()");
        }
        [Test, Category("T476018")]
        public void Attached_NotAttached() {
            ParserTestHelper.AssertSyntaxTreeString("($sys:DateTime.Now)", "(sys:DateTime.Now)");
            ParserTestHelper.AssertSyntaxTreeString("($DateTime.Now)", "(DateTime.Now)");
            ParserTestHelper.AssertSyntaxTreeString("($sys:DateTime.Now.Year)", "sys:DateTime.Now.Year");
            ParserTestHelper.AssertSyntaxTreeString("($DateTime.Now.Year)", "DateTime.Now.Year");
            ParserTestHelper.AssertSyntaxTreeString("($sys:DateTime.Now())", "sys:DateTime.Now()");
            ParserTestHelper.AssertSyntaxTreeString("($DateTime.Now())", "DateTime.Now()");
            ParserTestHelper.AssertSyntaxTreeString("($sys:DateTime.Now().Year)", "sys:DateTime.Now().Year");
            ParserTestHelper.AssertSyntaxTreeString("($DateTime.Now().Year)", "DateTime.Now().Year");
            ParserTestHelper.AssertSyntaxTreeString("($sys:DateTime.Now.Year - Row.HireDate.Year)", "sys:DateTime.Now.Year-Row.HireDate.Year");
        }
        [Test]
        public void TypeOf() {
            ParserTestHelper.AssertSyntaxTreeString("typeof ( $ Window )", "typeof(Window)");
            ParserTestHelper.AssertSyntaxTreeString("Method(typeof($Type))", "Method(typeof(Type))");
            ParserTestHelper.AssertSyntaxTreeString("($Type.Prop1).Method1(typeof($Type))", "(Type.Prop1).Method1(typeof(Type))");

            ParserTestHelper.AssertSyntaxTreeString("typeof ( $ dx : Window )", "typeof(dx:Window)");
            ParserTestHelper.AssertSyntaxTreeString("Method(typeof($dx:Type))", "Method(typeof(dx:Type))");
            ParserTestHelper.AssertSyntaxTreeString("($dx:Type.Prop1).Method1(typeof($dx:Type))", "(dx:Type.Prop1).Method1(typeof(dx:Type))");

            ParserTestHelper.AssertSyntaxTreeString("typeof($dx:Window).GetMethod()", "typeof(dx:Window).GetMethod()");

            ParserTestHelper.AssertArithmetic("typeof(int)", typeof(int));
        }
        [Test]
        [SetCulture("ru-RU")]
        public void ConstantWithCulture() {
            Constant();
        }
        [Test]
        public void Constant() {
            ParserTestHelper.AssertConstant("4", NConstant.NKind.Integer, 4);
            ParserTestHelper.AssertConstant("4U", NConstant.NKind.Integer, 4U);
            ParserTestHelper.AssertConstant("4u", NConstant.NKind.Integer, 4u);
            ParserTestHelper.AssertConstant("4L", NConstant.NKind.Integer, 4L);
            ParserTestHelper.AssertConstant("4UL", NConstant.NKind.Integer, 4UL);
            ParserTestHelper.AssertConstant("4Ul", NConstant.NKind.Integer, 4Ul);
            ParserTestHelper.AssertConstant("4uL", NConstant.NKind.Integer, 4uL);
            ParserTestHelper.AssertConstant("4ul", NConstant.NKind.Integer, 4ul);
            ParserTestHelper.AssertConstant("4LU", NConstant.NKind.Integer, 4LU);
            ParserTestHelper.AssertConstant("4Lu", NConstant.NKind.Integer, 4Lu);
            ParserTestHelper.AssertConstant("0x1", NConstant.NKind.Integer, 0x1);
            ParserTestHelper.AssertConstant("0X1", NConstant.NKind.Integer, 0X1);
            ParserTestHelper.AssertConstant("0x1U", NConstant.NKind.Integer, 0x1U);

            ParserTestHelper.AssertConstant(".1", NConstant.NKind.Float, .1);
            ParserTestHelper.AssertConstant("1.1", NConstant.NKind.Float, 1.1);
            ParserTestHelper.AssertConstant("1e1", NConstant.NKind.Float, 1e1);
            ParserTestHelper.AssertConstant("1e+1", NConstant.NKind.Float, 1e+1);
            ParserTestHelper.AssertConstant("1e-1", NConstant.NKind.Float, 1e-1);
            ParserTestHelper.AssertConstant(".1e1", NConstant.NKind.Float, .1e1);
            ParserTestHelper.AssertConstant(".1e+1", NConstant.NKind.Float, .1e+1);
            ParserTestHelper.AssertConstant(".1e-1", NConstant.NKind.Float, .1e-1);

            ParserTestHelper.AssertConstant("1F", NConstant.NKind.Float, 1F);
            ParserTestHelper.AssertConstant("1f", NConstant.NKind.Float, 1f);
            ParserTestHelper.AssertConstant("1D", NConstant.NKind.Float, 1D);
            ParserTestHelper.AssertConstant("1d", NConstant.NKind.Float, 1d);
            ParserTestHelper.AssertConstant("1M", NConstant.NKind.Float, 1M);
            ParserTestHelper.AssertConstant("1m", NConstant.NKind.Float, 1m);
            ParserTestHelper.AssertConstant(".1e-1F", NConstant.NKind.Float, .1e-1F);

            ParserTestHelper.AssertConstant("true", NConstant.NKind.Boolean, true);
            ParserTestHelper.AssertConstant("false", NConstant.NKind.Boolean, false);

            ParserTestHelper.AssertConstant("null", NConstant.NKind.Null, null);

            ParserTestHelper.AssertConstant("`test`", NConstant.NKind.String, "test", false);
            ParserTestHelper.AssertSyntaxTreeString("`test`", "\"test\"");
        }
        [Test]
        public void Unary() {
            ParserTestHelper.AssertSyntaxTreeString("-1");
            ParserTestHelper.AssertSyntaxTreeString("+1", "1");
            ParserTestHelper.AssertSyntaxTreeString("--+1", "--1");
            ParserTestHelper.AssertSyntaxTreeString("++1", "1");
            ParserTestHelper.AssertArithmetic("!false", true); ParserTestHelper.AssertArithmetic("!a", true, "a", new[] { false });
            ParserTestHelper.AssertArithmetic("~false", true); ParserTestHelper.AssertArithmetic("~a", true, "a", new[] { false });
            ParserTestHelper.AssertArithmetic("(a) + 1", 2, "a", new[] { 1 });
        }
        [Test]
        public void Cast() {
            ParserTestHelper.AssertSyntaxTreeString("(sbyte)(1)", "(sbyte)1");
            ParserTestHelper.AssertArithmetic("(sbyte)(2.2+1)", (sbyte)3);
            ParserTestHelper.AssertArithmetic("( sbyte)( 2.2+ 1)", (sbyte)3);
            ParserTestHelper.AssertArithmetic("(  sbyte) ( byte ) (2.1)", (sbyte)2);
            ParserTestHelper.AssertArithmetic("- - - + ( int) (byte ) (2.1)", - - -+(int)(byte)(2.1));
            ParserTestHelper.AssertArithmetic("- - - + ( long) (int ) -2.1", - - -+(long)(int)-2);
            ParserTestHelper.AssertSyntaxTreeString("($Type)1", "(Type)1");
            ParserTestHelper.AssertSyntaxTreeString("($x:Type)1", "(x:Type)1");

            ParserTestHelper.AssertSyntaxTreeString("1 is int", "(1 is int)");
            ParserTestHelper.AssertSyntaxTreeString("1 as int", "(1 as int)");
            ParserTestHelper.AssertArithmetic("1 is int", true);
            ParserTestHelper.AssertArithmetic("1 is double", false);
            ParserTestHelper.AssertArithmetic("1d is double", true);
            ParserTestHelper.AssertArithmetic("1d is double?", true);
            ParserTestHelper.AssertArithmetic("1 is double?", false);
            ParserTestHelper.AssertArithmetic("1 as double?", null);
            ParserTestHelper.AssertArithmetic("1d as double?", 1d);

            ParserTestHelper.AssertArithmetic("(double?)@c ?? 2", (double)2, null, new object[] { null }, null);
            ParserTestHelper.AssertArithmetic("(double?)@c ?? 2", (double)1, null, new object[] { 1 }, null);
        }
        [Test]
        public void Relative() {
            ParserTestHelper.AssertSyntaxTreeString("@DataContext", "");
            ParserTestHelper.AssertSyntaxTreeString("@Self");
            ParserTestHelper.AssertSyntaxTreeString("@TemplatedParent");
            ParserTestHelper.AssertSyntaxTreeString("@ElementName(ElName)");
            ParserTestHelper.AssertSyntaxTreeString("@StaticResource(ResName)");
            ParserTestHelper.AssertSyntaxTreeString("@FindAncestor($x:Window,1)", "@FindAncestor(x:Window,1)");

            ParserTestHelper.AssertSyntaxTreeString("@c", "");
            ParserTestHelper.AssertSyntaxTreeString("@c.Prop", ".Prop");
            ParserTestHelper.AssertSyntaxTreeString("@c.Method()", ".Method()");

            ParserTestHelper.AssertSyntaxTreeString("@s");
            ParserTestHelper.AssertSyntaxTreeString("@s.Prop");
            ParserTestHelper.AssertSyntaxTreeString("@s.Method()");

            ParserTestHelper.AssertSyntaxTreeString("@p");
            ParserTestHelper.AssertSyntaxTreeString("@p.Prop");
            ParserTestHelper.AssertSyntaxTreeString("@p.Method()");

            ParserTestHelper.AssertSyntaxTreeString("@e(ElementName)");
            ParserTestHelper.AssertSyntaxTreeString("@e(ElementName).Prop");
            ParserTestHelper.AssertSyntaxTreeString("@e(ElementName).Method()");

            ParserTestHelper.AssertSyntaxTreeString("@r(ResourceName)");
            ParserTestHelper.AssertSyntaxTreeString("@r(ResourceName).Prop");
            ParserTestHelper.AssertSyntaxTreeString("@r(ResourceName).Method()");

            ParserTestHelper.AssertSyntaxTreeString("@a($Window)", "@a(Window)");
            ParserTestHelper.AssertSyntaxTreeString("@a($Window).Prop", "@a(Window).Prop");
            ParserTestHelper.AssertSyntaxTreeString("@a($Window).Method()", "@a(Window).Method()");

            ParserTestHelper.AssertSyntaxTreeString("@a($x:Window)", "@a(x:Window)");
            ParserTestHelper.AssertSyntaxTreeString("@a($x:Window).Prop", "@a(x:Window).Prop");
            ParserTestHelper.AssertSyntaxTreeString("@a($x:Window).Method()", "@a(x:Window).Method()");

            ParserTestHelper.AssertSyntaxTreeString("@a($Window,2)", "@a(Window,2)");
            ParserTestHelper.AssertSyntaxTreeString("@a($Window,2).Prop", "@a(Window,2).Prop");
            ParserTestHelper.AssertSyntaxTreeString("@a($Window,2).Method()", "@a(Window,2).Method()");

            ParserTestHelper.AssertSyntaxTreeString("Method(@s.Prop,@a($x:Window,2))", "Method(@s.Prop,@a(x:Window,2))");

            ParserTestHelper.AssertArithmetic("@s", 1, "", new[] { 1 });
            ParserTestHelper.AssertArithmetic("@s.Prop", 1, "Prop", new[] { 1 });
            ParserTestHelper.AssertArithmetic("@c", 1, "", new[] { 1 });
            ParserTestHelper.AssertArithmetic("@c.Prop", 1, "Prop", new[] { 1 });

            ParserTestHelper.AssertSyntaxTreeString("$int.Parse(@s.Text)", "int.Parse(@s.Text)");
            ParserTestHelper.AssertArithmetic("$int.Parse(@s.Text)", 1, null, new[] { "1" });
        }
        [Test]
        public void Index() {
            ParserTestHelper.AssertSyntaxTreeString("[1]");
            ParserTestHelper.AssertSyntaxTreeString("[1][2]", "[1].[2]");
            ParserTestHelper.AssertSyntaxTreeString("a[1].A", "a.[1].A");
            ParserTestHelper.AssertSyntaxTreeString("a[1,1][2].A", "a.[1,1].[2].A");
            ParserTestHelper.AssertSyntaxTreeString("a()[1].A", "a().[1].A");
            ParserTestHelper.AssertSyntaxTreeString("a(a[1], b[2])[1].A", "a(a.[1],b.[2]).[1].A");
            ParserTestHelper.AssertSyntaxTreeString("a.b[1]", "a.b.[1]");
            ParserTestHelper.AssertSyntaxTreeString("$Type.Prop[1]", "Type.Prop.[1]");
            ParserTestHelper.AssertSyntaxTreeString("($Type.Prop)[1]", "(Type.Prop).[1]");
            ParserTestHelper.AssertSyntaxTreeString("(1+2)[1]", "(1+2).[1]");

            ParserTestHelper.AssertOperands("a[1].A", "a");
            ParserTestHelper.AssertOperands("a.GetSelf()[1].A", "a");

            ParserTestHelper.AssertArithmetic("a.GetSelf().Array[1]", "b", null,
                new[] { new ParserTests_a(array: new string[] { "a", "b" }) });
        }
        [Test]
        public void OperandsOrder() {
            ParserTestHelper.AssertOperands("-a*b/(+c*-d)/e", "a;b;c;d;e");
            ParserTestHelper.AssertOperands("a*b/c*d", "a;b;c;d");
            ParserTestHelper.AssertOperands("a == b ? c && d ? e : f : g", "a;b;c;d;e;f;g");
        }
        [Test]
        public void ArithmeticNumeric() {
            ParserTestHelper.AssertArithmetic("-1", -1);
            ParserTestHelper.AssertArithmetic("+1", 1);
            ParserTestHelper.AssertArithmetic("--+1", 1);
            ParserTestHelper.AssertArithmetic("++1", 1);
            ParserTestHelper.AssertArithmetic("1+2", 3); ParserTestHelper.AssertArithmetic("a+b", 3, "a;b", new[] { 1, 2 });
            ParserTestHelper.AssertArithmetic("1+2*2", 5); ParserTestHelper.AssertArithmetic("a+b*c", 5, "a;b;c", new[] { 1, 2, 2 });
            ParserTestHelper.AssertArithmetic("(1+2)*2", 6); ParserTestHelper.AssertArithmetic("(a+b)*c", 6, "a;b;c", new[] { 1, 2, 2 });
            ParserTestHelper.AssertArithmetic("2*2/4*3", 3); ParserTestHelper.AssertArithmetic("a*b/c*d", 3, "a;b;c;d", new[] { 2, 2, 4, 3 });
            ParserTestHelper.AssertArithmetic("2.1*2", 4.2); ParserTestHelper.AssertArithmetic("a*b", 4.2, "a;b", new[] { 2.1, 2 });
            ParserTestHelper.AssertArithmetic("2*2.1", 4.2); ParserTestHelper.AssertArithmetic("a*b", 4.2, "a;b", new[] { 2, 2.1 });
            ParserTestHelper.AssertArithmetic("5/2", 2); ParserTestHelper.AssertArithmetic("a/b", 2, "a;b", new[] { 5, 2 });
            ParserTestHelper.AssertArithmetic("5/2d", 2.5); ParserTestHelper.AssertArithmetic("a/2d", 2.5, "a", new[] { 5 });
            ParserTestHelper.AssertArithmetic("-4*2/(+2*2)/2", -1); ParserTestHelper.AssertArithmetic("-a*b/(+c*d)/e", -1, "a;b;c;d;e", new[] { 4, 2, 2, 2, 2 });
            ParserTestHelper.AssertArithmetic("-4*2/(+2*-2)/2", 1); ParserTestHelper.AssertArithmetic("-a*b/(+c*d)/e", 1, "a;b;c;d;e", new[] { 4, 2, 2, -2, 2 });
            ParserTestHelper.AssertArithmetic("--+4", 4); ParserTestHelper.AssertArithmetic("--+a", 4, "a", new[] { 4 });
            ParserTestHelper.AssertArithmetic("10>>2", 10 >> 2); ParserTestHelper.AssertArithmetic("a>>b", 10 >> 2, "a;b", new[] { 10, 2 });
            ParserTestHelper.AssertArithmetic("10<<2", 10 << 2); ParserTestHelper.AssertArithmetic("a<<b", 10 << 2, "a;b", new[] { 10, 2 });
            ParserTestHelper.AssertArithmetic("10<<-2", 10 << -2); ParserTestHelper.AssertArithmetic("a<<-b", 10 << -2, "a;b", new[] { 10, 2 });

            ParserTestHelper.AssertArithmetic("3&2", 3 & 2);
            ParserTestHelper.AssertArithmetic("3|2", 3 | 2);
            ParserTestHelper.AssertArithmetic("3^2", 3 ^ 2);
            ParserTestHelper.AssertArithmetic("1&2|3^4", 1 & 2 | 3 ^ 4);
            ParserTestHelper.AssertArithmetic("true&true^true", true & true ^ true);
            ParserTestHelper.AssertArithmetic("true&false^true", true & false ^ true);
            ParserTestHelper.AssertArithmetic("true&false|true", true & false | true);

            ParserTestHelper.AssertArithmetic("`a`+`b`", "ab"); ParserTestHelper.AssertArithmetic("`a`+b", "ab", "b", new[] { "b" });
            ParserTestHelper.AssertArithmetic("1+1+2", 4); ParserTestHelper.AssertArithmetic("a+a+b", 4, "a;b", new[] { 1, 2 });
        }
        [Test]
        public void ArithmeticEquality() {
            ParserTestHelper.AssertArithmetic("2 > 1.1", 2 > 1.1);
            ParserTestHelper.AssertArithmetic("1.1 > 2", 1.1 > 2);
            ParserTestHelper.AssertArithmetic("2d > 2", 2d > 2);
            ParserTestHelper.AssertArithmetic("2d < 2", 2d < 2);
            ParserTestHelper.AssertArithmetic("2d >= 2", 2d >= 2);
            ParserTestHelper.AssertArithmetic("2d <= 2", 2d <= 2);

            ParserTestHelper.AssertArithmetic("1 == 2", 1 == 2);
            ParserTestHelper.AssertArithmetic("1 != 2", 1 != 2);
            ParserTestHelper.AssertArithmetic("1.1 == 2", 1.1 == 2);
            ParserTestHelper.AssertArithmetic("1 != 2.1", 1 != 2.1);
            ParserTestHelper.AssertArithmetic("1 == 1", 1 == 1);
            ParserTestHelper.AssertArithmetic("1d == 1", 1d == 1);
            ParserTestHelper.AssertArithmetic("1d == 1d", 1d == 1d);
            ParserTestHelper.AssertArithmetic("1 != 1", 1 != 1);
            ParserTestHelper.AssertArithmetic("1d != 1", 1d != 1);
            ParserTestHelper.AssertArithmetic("1d != 1d", 1d != 1d);
        }
        [Test]
        public void ArithmeticLogical() {
            ParserTestHelper.AssertArithmetic("(true || true) && false", (true || true) && false);
            ParserTestHelper.AssertArithmetic("true || (true && false)", true || (true && false));
            ParserTestHelper.AssertArithmetic("true || true && false", true || true && false);
            ParserTestHelper.AssertArithmetic("true == false", true == false);
            ParserTestHelper.AssertArithmetic("true != false", true != false);

            ParserTestHelper.AssertArithmetic("!true", !true);
            ParserTestHelper.AssertArithmetic("!false", !false);
            ParserTestHelper.AssertArithmetic("~4", ~4);

            ParserTestHelper.AssertSyntaxTreeString("4??5");
            ParserTestHelper.AssertArithmetic("true ? true : false", true);
            ParserTestHelper.AssertArithmetic("1 == 2 ? true : false", false);
            ParserTestHelper.AssertArithmetic("1 == 2 ? true : 1 == 2", false);
            ParserTestHelper.AssertArithmetic("1 == 1 ? true && true : false", true);
            ParserTestHelper.AssertArithmetic("1 == 1 ? true && true ? 1 : 2 : 3", 1 == 1 ? true && true ? 1 : 2 : 3);
        }
        [Test, Category("T491236")]
        public void TernaryOperations() {
            ParserTestHelper.AssertSyntaxTreeString("@e(el1).Pr1?@e(el2).Pr2:@e(el3).Pr3");
            ParserTestHelper.AssertArithmetic("@e(el1).BoolProp?@e(el2).IntProp:@e(el3).IntProp",
                2, "BoolProp;IntProp;IntProp", new object[] { true, 2, 3 });
            ParserTestHelper.AssertArithmetic("@e(el1).BoolProp?@e(el2).IntProp:@e(el3).IntProp",
                3, "BoolProp;IntProp;IntProp", new object[] { false, 2, 3 });
        }

        [Test]
        public void BackMode_Basic() {
            ParserTestHelper.BackMode_AssertSyntaxTreeString("a=1");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("b=@v;; ; ", "b=@v");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("c=@value");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("a=1;b=2");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("a=1;;;b=2;;c=3;", "a=1;b=2;c=3");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("($dx:Type.Prop)=@v?@v.Prop+@value:@v", "(dx:Type.Prop)=@v?@v.Prop+@value:@v");

            ParserTestHelper.BackMode_AssertSyntaxTreeString("1");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("@v");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("!@v");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("2+@v");
        }
        [Test]
        public void BackMode_Operand() {
            IList<Operand> operands;
            operands = ParserTestHelper.GetOperands("Prop", "@v").ToList();
            Assert.AreEqual(1, operands.Count);
            ParserTestHelper.AssertOperand(operands[0], "Prop", true);

            operands = ParserTestHelper.GetOperands("Prop", "Prop=@v").ToList();
            Assert.AreEqual(1, operands.Count);
            ParserTestHelper.AssertOperand(operands[0], "Prop", true);

            operands = ParserTestHelper.GetOperands("Prop1", "Prop2=@v").ToList();
            Assert.AreEqual(2, operands.Count);
            ParserTestHelper.AssertOperand(operands[0], "Prop1", false);
            ParserTestHelper.AssertOperand(operands[1], "Prop2", true);

            operands = ParserTestHelper.GetOperands("Prop1+Prop2", "Prop3=@v").ToList();
            Assert.AreEqual(3, operands.Count);
            ParserTestHelper.AssertOperand(operands[0], "Prop1", false);
            ParserTestHelper.AssertOperand(operands[1], "Prop2", false);
            ParserTestHelper.AssertOperand(operands[2], "Prop3", true);


            operands = ParserTestHelper.GetOperands("Prop1+Prop2", "Prop2=@v;Prop3=@v").ToList();
            Assert.AreEqual(3, operands.Count);
            ParserTestHelper.AssertOperand(operands[0], "Prop1", false);
            ParserTestHelper.AssertOperand(operands[1], "Prop2", true);
            ParserTestHelper.AssertOperand(operands[2], "Prop3", true);
        }

        [Test]
        public void InvalidProperty() {
            ParserTestHelper.AssertValidation("GetSelf().MethodA(1)", null,
                new object[] { null }, null);
            ParserTestHelper.AssertValidation("GetSelf().MethodA(1)", 1,
                new object[] { new ValidationTests_a() }, null);
            ParserTestHelper.AssertValidation("GetSelf().MethodA(1)", null,
                    new object[] { new ValidationTests_b() },
                    @"The 'MethodA(Int32)' method is not found on object 'ValidationTests_b'.");
        }
        [Test]
        public void DefaultErrorTest() {
            Func<int, string> getError =
                x => ErrorHelper.ReportParserError(-1, Errors.GetDefaultError(x), ParserMode.BindingExpr);
            Action<string, int> assert1 =
                (x, y) => {
                    Assert.AreEqual("DXBinding error: " + x + ".", getError(y));
                };

            assert1("EOF expected", 0);
            assert1("identifier expected", 1);
            assert1("integer expected", 2);
            assert1("float expected", 3);
            assert1("string expected", 4);

            int ending = 71;
            Assert.AreEqual("??? expected", Errors.GetDefaultError(ending++));
            assert1("invalid expression", ending++);
            assert1("invalid expression", ending++);
            assert1("invalid type expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid constant expression", ending++);
            assert1("invalid expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid assignment expression", ending++);
            assert1("invalid expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid expression", ending++);
            assert1("invalid identifier expression", ending++);
            assert1("invalid expression", ending++);
        }
        [Test]
        public void AcceptEscapedStringLiterals() {
            ParserTestHelper.BackMode_AssertSyntaxTreeString("a = `\\``", "a=\"`\"");
            ParserTestHelper.BackMode_AssertSyntaxTreeString("a = `a\\`\\n\\t' \\0 \\a \\b \\f \\r \\v`", "a=\"a`\n\t' \0 \a \b \f \r \v\"");
        }
        [Test, Category("T504788")]
        public void MethodParameters() {
            ParserTestHelper.AssertArithmetic(
                "$sys:String.Format(`Page: {0} of {1}`, IntProp, DoubleProp)",
                "Page: 1 of 2",
                null,
                new[] { 1, 2d },
                x => typeof(string));
        }
        [Test]
        public void AttachedOperands() {
            ParserTestHelper.AssertArithmetic(
                "($x:Class.AttachedProp)",
                1,
                null,
                new object[] { 1 },
                null);
            ParserTestHelper.AssertArithmetic(
                "$sys:Math.Max(($x:Class.AttachedProp1), ($x:Class.AttachedProp2))",
                2,
                null,
                new object[] { 1, 2 },
                x => typeof(Math));
            ParserTestHelper.AssertArithmetic(
                "$sys:Math.Max(@e(element1).($x:Class.AttachedProp1), @e(element2).($x:Class.AttachedProp2))",
                2,
                null,
                new object[] { 1, 2 },
                x => typeof(Math));
        }
    }
    public class ParserTests_a {
        public static int StaticIntProp { get; set; }
        public static int StaticIntField { get; set; }
        public static ParserTests_a StaticSelf { get; set; }
        public static void Static(int intV = 0) {
            StaticIntProp = StaticIntField = intV;
            StaticSelf = new ParserTests_a().CreateInstance();
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
        public ParserTests_a Self { get { return this; } }
        public ParserTests_a(int intV = 0, double doubleV = 0, string stringV = "", string[] array = null, Dictionary<Type, string> dictionary = null) {
            AtProp = @int = IntProp = IntField = intV;
            DoubleProp = DoubleField = doubleV;
            StringProp = StringField = stringV;
            Array = array;
            Dictionary = dictionary;
        }
        public ParserTests_a GetSelf() {
            return this;
        }
        public ParserTests_a GetSelf(ParserTests_a a) {
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

        protected virtual ParserTests_a CreateInstance() {
            return new ParserTests_a();
        }
    }

    public class ValidationTests_a {
        public ValidationTests_a GetSelf() { return this; }
        public int MethodA(int v) { return v; }
    }
    public class ValidationTests_b {
        public ValidationTests_b GetSelf() { return this; }
        public int MethodB(int v) { return v; }
    }

    [TestFixture]
    public class ParserTests_CommandToMethod {
        [Test]
        public void ExecuteBasicTest() {
            ParserTestHelper.Execute_AssertSyntaxTreeString("Prop1");
            ParserTestHelper.Execute_AssertSyntaxTreeString("Prop1;; ; ", "Prop1");
            ParserTestHelper.Execute_AssertSyntaxTreeString("Method()");
            ParserTestHelper.Execute_AssertSyntaxTreeString("Method(Prop1)");
            ParserTestHelper.Execute_AssertSyntaxTreeString("@s.Method(Prop1, @a($x:Type))", "@s.Method(Prop1,@a(x:Type))");
            ParserTestHelper.Execute_AssertSyntaxTreeString("[@s.Get()].Method()");
            ParserTestHelper.Execute_AssertSyntaxTreeString("Method1();Method2();Prop");
            ParserTestHelper.Execute_AssertSyntaxTreeString("Method1(@parameter)");

            Assert.Throws<AssertionException> (() => {
                ParserTestHelper.Execute_AssertSyntaxTreeString("@parameter");
            });
            Assert.Throws<AssertionException>(() => {
                ParserTestHelper.Execute_AssertSyntaxTreeString("1");
            });
            ParserTestHelper.Execute_AssertSyntaxTreeString("Method(1)");
        }
        [Test]
        public void CanExecuteBasicTest() {
            ParserTestHelper.CanExecute_AssertSyntaxTreeString("true && false", "True&&False");
            ParserTestHelper.CanExecute_AssertSyntaxTreeString("Prop1.CanMethod(@e(ElementName),@parameter.Something())");
        }
        [Test]
        public void ExecuteArithmetic() {
            var vm = new ParserTests_CommandToMethod_a();
            ParserTestHelper.Execute_AssertArithmetic("Execute1(true)", new[] { vm }, null);
            Assert.AreEqual(true, vm.Execute1Res);
            vm.Tag = false;
            ParserTestHelper.Execute_AssertArithmetic("Execute1(Tag)", new[] { vm, vm.Tag }, null);
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
            Action<string> ex = x => ParserTestHelper.Execute_AssertArithmetic(x, new[] { obj }, null);
            ex("A(1, 1)"); Assert.IsTrue(obj.A3); obj.Clear();
            ex("A(`1`, 1)"); Assert.IsTrue(obj.A2); obj.Clear();
            ex("A(`1`, `1`)"); Assert.IsTrue(obj.A1); obj.Clear();
            ex("A(1, `1`)"); Assert.IsTrue(obj.A4); obj.Clear();
        }
        [Test]
        public void ResearchStandardMethodSearchTest2() {
            var obj = new ResearchStandardMethodSearch();
            string o = "1";
            ResearchStandardMethodSearch.a1 a1 = new ResearchStandardMethodSearch.a1();
            ResearchStandardMethodSearch.a2 a2 = new ResearchStandardMethodSearch.a2();
            ResearchStandardMethodSearch.a3 a3 = new ResearchStandardMethodSearch.a3();

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
        public void MethodSearchTest2() {
            var obj = new ResearchStandardMethodSearch();
            string o = "1";
            ResearchStandardMethodSearch.a1 a1 = new ResearchStandardMethodSearch.a1();
            ResearchStandardMethodSearch.a2 a2 = new ResearchStandardMethodSearch.a2();
            ResearchStandardMethodSearch.a3 a3 = new ResearchStandardMethodSearch.a3();
            ResearchStandardMethodSearch.a4 a4 = new ResearchStandardMethodSearch.a4();

            Action<string> ex = x => ParserTestHelper.Execute_AssertArithmetic("o; a1; a2; a3; a4; " + x, new object[] { o, a1, a2, a3, a4, obj }, null);

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
            ParserTestHelper.Execute_AssertValidation("o; a1; a2; a3; a4; " + "B(a3, a3)",
                new object[] { o, a1, a2, a3, a4, obj }, "The 'B(a3, a3)' method is not found on object 'ResearchStandardMethodSearch'.");
            ParserTestHelper.Execute_AssertValidation("o; a1; a2; a3; a4; " + "C(a4, a4)",
                new object[] { o, a1, a2, a3, a4, obj }, "The 'C(a4, a4)' method is not found on object 'ResearchStandardMethodSearch'.");
        }
        class ResearchStandardMethodSearch {
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
            public void B(object p1, a1 p2) { B2 = true; }
            public void B(object p1, a2 p2) { B3 = true; }
            public void B(a1 p1, object p2) { B5 = true; }
            public void B(a1 p1, a1 p2) { B6 = true; }
            public void B(a1 p1, a2 p2) { B7 = true; }
            public void B(a1 p1, a3 p2) { B8 = true; }
            public void B(a2 p1, object p2) { B9 = true; }
            public void B(a2 p1, a1 p2) { B10 = true; }
            public void B(a2 p1, a2 p2) { B11 = true; }
            public void B(a2 p1, a3 p2) { B12 = true; }
            public void B(a3 p1, object p2) { B13 = true; }
            public void B(a3 p1, a1 p2) { B14 = true; }
            public void B(a3 p1, a2 p2) { B15 = true; }
#endregion
#region C
            public void C(a2 p1, a1 p2) { }
            public void C(a1 p1, a3 p2) { }
#endregion

            public void Clear() {
                A1 = A2 = A3 = A4 = A5 = false;
                B1 = B2 = B3 = B4 = B5 = B6 = B7 = B8 = B9 = B10 = B11 = B12 = B13 = B14 = B15 = B16 = false;
            }

            public class a1 { }
            public class a2 : a1 { }
            public class a3 : a2 { }
            public class a4 : a3 { }
        }
#endregion
    }
    public class ParserTests_CommandToMethod_a {
        public object Tag { get; set; }
        public object Execute1Res { get; set; }
        public object Execute2Res { get; set; }
        public void Execute1(object res) {
            Execute1Res = res;
        }
    }
}