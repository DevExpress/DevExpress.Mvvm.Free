using System;
using System.Collections.Generic;
using System.Linq;

enum ParserMode { BindingExpr, BindingBackExpr, CommandExecute, CommandCanExecute, Event }



namespace DevExpress.DXBinding.Native {


class Parser : ParserBase {
 public Parser(Scanner scanner, IParserErrorHandler errorHandler) : base(scanner, errorHandler) { }
 protected override int GetMaxTokenKind() { return maxT; }
 public const int _EOF = 0;
 public const int _Ident = 1;
 public const int _Int = 2;
 public const int _Float = 3;
 public const int _String = 4;
 public const int maxT = 72;

 protected override bool IsEOF(int n) {
  return n == _EOF;
 }
public ParserMode Mode { get; set; }
 public NRoot Root { get { return root; } }
 NRoot root;

 bool NextIs_MethodExpr(int pos) {
  return TokenEquals(pos, _Ident) && TokenEquals(pos + 1, "(");
 }
 bool NextIs_IdentExpr(int pos) {
  return TokenEquals(pos, _Ident) && !TokenEquals(pos + 1, "(");
 }
 bool NextIs_TypeExprWrapped(int pos, string lParen, string rParen) {
  if(!TokenEquals(pos, lParen)) return false;
  int length;
  if(!NextIs_TypeExpr(pos+1, out length)) return false;
  if(!TokenEquals(pos+1+length, rParen)) return false;
  return true;
 }
 bool NextIs_AttachedPropExpr(int pos) {
  if(!TokenEquals(pos, "(")) return false;
  int length;
  if(!NextIs_TypeExpr(pos+1, out length)) return false;
  if(!TokenEquals(pos+1+length, ".")) return false;
  if(!TokenEquals(pos+2+length, _Ident)) return false;
  if(!TokenEquals(pos+3+length, ")")) return false;
  return true;
 }
 bool NextIs_TypeExpr(int pos) {
  int length; return NextIs_TypeExpr(pos, out length);
 }
 bool NextIs_TypeExpr(int pos, out int length) {
  length = 0;
  if(TokenEquals(pos + length, NType.PrimitiveTypes)) {
   length++;
   if(TokenEquals(pos + length, "?")) length++;
   return true;
  }
  if(TokenEquals(pos + length, "$") && TokenEquals(pos + length + 1, _Ident)) length+=2;
  else { length = 0; return false; }
  if(TokenEquals(pos + length, ":") && TokenEquals(pos + length + 1, _Ident)) {
   length += 2;
   if(TokenEquals(pos + length, "?")) length++;
  }
  return true;
 }

 protected override ErrorsBase CreateErrorHandler(IParserErrorHandler errorHandler) {
  return new Errors(errorHandler);
 }
 protected override void Pragmas() {

 }
 void DXBinding() {
  if (Mode == ParserMode.BindingExpr) {
   ExprRoot();
  } else if (Mode == ParserMode.BindingBackExpr) {
   Back_ExprRoot();
  } else if (Mode == ParserMode.CommandExecute) {
   Execute_ExprRoot();
  } else if (Mode == ParserMode.CommandCanExecute) {
   CanExecute_ExprRoot();
  } else if (Mode == ParserMode.Event) {
   Event_ExprRoot();
  } else SynErr(73);
  Expect(0);
 }

 void ExprRoot() {
  root = new NRoot(); NBase res;
  Expr(out res);
  root.Exprs.Add(res);
 }

 void Back_ExprRoot() {
  root = new NRoot(); NBase res;
  if (TokenExists(0, "=")) {
   Back_Assign(out res);
   while (la.kind == 5) {
    Get();
   }
   root.Exprs.Add(res);
   while (TokenEquals(0, ";") && !TokenEquals(1, _EOF)) {
    Back_Assign(out res);
    while (la.kind == 5) {
     Get();
    }
    root.Exprs.Add(res);
   }
  } else if (StartOf(1)) {
   Expr(out res);
   root.Exprs.Add(res);
  } else SynErr(74);
 }

 void Execute_ExprRoot() {
  Assign_ExprRoot();
 }

 void CanExecute_ExprRoot() {
  ExprRoot();
 }

 void Event_ExprRoot() {
  Assign_ExprRoot();
 }

 void Expr(out NBase res) {
  ConditionExpr(out res);
 }

 void Assign_ExprRoot() {
  root = new NRoot(); NBase expr;
  Assign_Expr(out expr);
  while (la.kind == 5) {
   Get();
  }
  root.Exprs.Add(expr);
  while (TokenEquals(0, ";") && !TokenEquals(1, _EOF)) {
   Assign_Expr(out expr);
   while (la.kind == 5) {
    Get();
   }
   root.Exprs.Add(expr);
  }
 }

 void Assign_Expr(out NBase res) {
  res = null; NBase left = null; NBase expr = null;
  if (TokenExists(0, "=")) {
   Expr(out left);
   Expect(6);
   Expr(out expr);
   res = new NAssign(left, expr);
  } else if (StartOf(1)) {
   Expr(out res);
  } else SynErr(75);
 }

 void ConditionExpr(out NBase res) {
  res = null; NBase second = null; NBase third = null;
  NullCoalescingExpr(out res);
  if (la.kind == 7) {
   Get();
   Expr(out second);
   Expect(8);
   Expr(out third);
   res = new NTernary(NTernary.NKind.Condition, res, second, third);
  }
 }

 void NullCoalescingExpr(out NBase res) {
  res = null; NBase right = null;
  OrExpr(out res);
  while (la.kind == 9) {
   Get();
   OrExpr(out right);
   res = new NBinary(NBinary.NKind.Coalesce, res, right);
  }
 }

 void OrExpr(out NBase res) {
  res = null; NBase right = null;
  AndExpr(out res);
  while (la.kind == 10 || la.kind == 11) {
   if (la.kind == 10) {
    Get();
   } else {
    Get();
   }
   AndExpr(out right);
   res = new NBinary(NBinary.NKind.OrElse, res, right);
  }
 }

 void AndExpr(out NBase res) {
  res = null; NBase right = null;
  BitOrExpr(out res);
  while (la.kind == 12 || la.kind == 13) {
   if (la.kind == 12) {
    Get();
   } else {
    Get();
   }
   BitOrExpr(out right);
   res = new NBinary(NBinary.NKind.AndAlso, res, right);
  }
 }

 void BitOrExpr(out NBase res) {
  res = null; NBase right = null;
  BitXorExpr(out res);
  while (la.kind == 14) {
   Get();
   BitXorExpr(out right);
   res = new NBinary(NBinary.NKind.Or, res, right);
  }
 }

 void BitXorExpr(out NBase res) {
  res = null; NBase right = null;
  BitAndExpr(out res);
  while (la.kind == 15) {
   Get();
   BitAndExpr(out right);
   res = new NBinary(NBinary.NKind.Xor, res, right);
  }
 }

 void BitAndExpr(out NBase res) {
  res = null; NBase right = null;
  EqlExpr(out res);
  while (la.kind == 16) {
   Get();
   EqlExpr(out right);
   res = new NBinary(NBinary.NKind.And, res, right);
  }
 }

 void EqlExpr(out NBase res) {
  res = null; NBase right = null;
  RelExpr(out res);
  while (StartOf(2)) {
   if (la.kind == 17 || la.kind == 18) {
    if (la.kind == 17) {
     Get();
    } else {
     Get();
    }
    RelExpr(out right);
    res = new NBinary(NBinary.NKind.NotEqual, res, right);
   } else {
    if (la.kind == 19) {
     Get();
    } else {
     Get();
    }
    RelExpr(out right);
    res = new NBinary(NBinary.NKind.Equal, res, right);
   }
  }
 }

 void RelExpr(out NBase res) {
  res = null; NBase right = null;
  ShiftExpr(out res);
  while (StartOf(3)) {
   switch (la.kind) {
   case 21: case 22: {
    if (la.kind == 21) {
     Get();
    } else {
     Get();
    }
    ShiftExpr(out right);
    res = new NBinary(NBinary.NKind.Less, res, right);
    break;
   }
   case 23: case 24: {
    if (la.kind == 23) {
     Get();
    } else {
     Get();
    }
    ShiftExpr(out right);
    res = new NBinary(NBinary.NKind.Greater, res, right);
    break;
   }
   case 25: case 26: {
    if (la.kind == 25) {
     Get();
    } else {
     Get();
    }
    ShiftExpr(out right);
    res = new NBinary(NBinary.NKind.LessOrEqual, res, right);
    break;
   }
   case 27: case 28: {
    if (la.kind == 27) {
     Get();
    } else {
     Get();
    }
    ShiftExpr(out right);
    res = new NBinary(NBinary.NKind.GreaterOrEqual, res, right);
    break;
   }
   case 29: {
    Get();
    TypeExpr(out right);
    res = new NCast(NCast.NKind.Is, res, (NType)right);
    break;
   }
   case 30: {
    Get();
    TypeExpr(out right);
    res = new NCast(NCast.NKind.As, res, (NType)right);
    break;
   }
   }
  }
 }

 void ShiftExpr(out NBase res) {
  res = null; NBase right = null;
  AddExpr(out res);
  while (StartOf(4)) {
   if (la.kind == 31 || la.kind == 32) {
    if (la.kind == 31) {
     Get();
    } else {
     Get();
    }
    AddExpr(out right);
    res = new NBinary(NBinary.NKind.ShiftLeft, res, right);
   } else {
    if (la.kind == 33) {
     Get();
    } else {
     Get();
    }
    AddExpr(out right);
    res = new NBinary(NBinary.NKind.ShiftRight, res, right);
   }
  }
 }

 void TypeExpr(out NBase res) {
  res = null; string type = string.Empty;
  if (TokenEquals(1, NType.PrimitiveTypes)) {
   Expect(1);
   type = t.val;
  } else if (la.kind == 60) {
   Get();
   Expect(1);
   type = t.val;
   if (la.kind == 8) {
    Get();
    Expect(1);
    type = type + ":" + t.val;
   }
  } else SynErr(76);
  res = new NType(type, null, NType.NKind.Type, null);
  if (la.kind == 7) {
   Get();
   ((NType)res).IsNullable = true;
  }
 }

 void AddExpr(out NBase res) {
  res = null; NBase right = null;
  MulExpr(out res);
  while (la.kind == 35 || la.kind == 36) {
   if (la.kind == 35) {
    Get();
    MulExpr(out right);
    res = new NBinary(NBinary.NKind.Plus, res, right);
   } else {
    Get();
    MulExpr(out right);
    res = new NBinary(NBinary.NKind.Minus, res, right);
   }
  }
 }

 void MulExpr(out NBase res) {
  res = null; NBase right = null;
  UnaryExpr(out res);
  while (la.kind == 37 || la.kind == 38 || la.kind == 39) {
   if (la.kind == 37) {
    Get();
    UnaryExpr(out right);
    res = new NBinary(NBinary.NKind.Mul, res, right);
   } else if (la.kind == 38) {
    Get();
    UnaryExpr(out right);
    res = new NBinary(NBinary.NKind.Div, res, right);
   } else {
    Get();
    UnaryExpr(out right);
    res = new NBinary(NBinary.NKind.Mod, res, right);
   }
  }
 }

 void UnaryExpr(out NBase res) {
  res = null; List<NUnaryBase> ops = new List<NUnaryBase>();
  while (StartOf(1)) {
   if (la.kind == 35) {
    Get();
    ops.Add(new NUnary(NUnary.NKind.Plus, null));
   } else if (la.kind == 36) {
    Get();
    ops.Add(new NUnary(NUnary.NKind.Minus, null));
   } else if (la.kind == 40) {
    Get();
    ops.Add(new NUnary(NUnary.NKind.NotBitwise, null));
   } else if (la.kind == 41) {
    Get();
    ops.Add(new NUnary(NUnary.NKind.Not, null));
   } else if (NextIs_TypeExprWrapped(1, "(", ")")) {
    Expect(42);
    NBase type;
    TypeExpr(out type);
    ops.Add(new NCast(NCast.NKind.Cast, null, (NType)type));
    Expect(43);
   } else {
    break;
   }
  }
  AtomRootExpr(out res);
  for(int i = 0; i < ops.Count - 1; i++) ops[i].Value = ops[i+1];
  if(ops.Count > 0) { ops.Last().Value = res; res = ops.First(); }
 }

 void AtomRootExpr(out NBase res) {
  res = null;
  if (Mode == ParserMode.BindingExpr) {
   AtomExpr(out res);
  } else if (Mode == ParserMode.BindingBackExpr) {
   Back_AtomExpr(out res);
  } else if (Mode == ParserMode.CommandExecute) {
   Execute_AtomExpr(out res);
  } else if (Mode == ParserMode.CommandCanExecute) {
   CanExecute_AtomExpr(out res);
  } else if (Mode == ParserMode.Event) {
   Event_AtomExpr(out res);
  } else SynErr(77);
 }

 void AtomExpr(out NBase res) {
  res = null;
  if (StartOf(5)) {
   ConstExpr(out res);
  } else if (StartOf(6)) {
   RelativeExpr(out res);
  } else if (la.kind == 58) {
   TypeOfExpr(out res);
  } else if (la.kind == 61) {
   TypeNewExpr(out res);
  } else if (NextIs_TypeExpr(1)) {
   TypeIdentExpr(out res, true);
  } else if (NextIs_MethodExpr(1)) {
   MethodExpr(out res);
  } else if (la.kind == 1) {
   IdentExpr(out res);
  } else if (la.kind == 62) {
   IndexExpr(out res);
  } else if (la.kind == 42) {
   Get();
   if (NextIs_AttachedPropExpr(0)) {
    TypeIdentExpr(out res, false);
    ((NType)res).Kind = NType.NKind.Attached;
   } else if (StartOf(1)) {
    Expr(out res);
   } else SynErr(78);
   Expect(43);
  } else SynErr(79);
  ReadNextIdents(ref res, true);
 }

 void Back_AtomExpr(out NBase res) {
  res = null;
  if (StartOf(5)) {
   ConstExpr(out res);
  } else if (la.kind == 67 || la.kind == 68) {
   Back_RelativeValueExpr(out res);
  } else if (la.kind == 58) {
   TypeOfExpr(out res);
  } else if (la.kind == 61) {
   TypeNewExpr(out res);
  } else if (NextIs_TypeExpr(1)) {
   TypeIdentExpr(out res, true);
  } else if (la.kind == 42) {
   Get();
   Expr(out res);
   Expect(43);
  } else SynErr(80);
  ReadNextIdents(ref res, true);
 }

 void Execute_AtomExpr(out NBase res) {
  res = null;
  if (StartOf(5)) {
   ConstExpr(out res);
  } else if (StartOf(7)) {
   Execute_RelativeExpr(out res);
  } else if (la.kind == 58) {
   TypeOfExpr(out res);
  } else if (la.kind == 61) {
   TypeNewExpr(out res);
  } else if (NextIs_TypeExpr(1)) {
   TypeIdentExpr(out res, true);
  } else if (NextIs_MethodExpr(1)) {
   MethodExpr(out res);
  } else if (la.kind == 1) {
   IdentExpr(out res);
  } else if (la.kind == 62) {
   IndexExpr(out res);
  } else if (la.kind == 42) {
   Get();
   if (NextIs_TypeExpr(1)) {
    TypeIdentExpr(out res, false);
    ((NType)res).Kind = NType.NKind.Attached;
   } else if (StartOf(1)) {
    Expr(out res);
   } else SynErr(81);
   Expect(43);
  } else SynErr(82);
  ReadNextIdents(ref res, true);
 }

 void CanExecute_AtomExpr(out NBase res) {
  Execute_AtomExpr(out res);
 }

 void Event_AtomExpr(out NBase res) {
  res = null;
  if (StartOf(5)) {
   ConstExpr(out res);
  } else if (StartOf(8)) {
   Event_RelativeExpr(out res);
  } else if (la.kind == 58) {
   TypeOfExpr(out res);
  } else if (la.kind == 61) {
   TypeNewExpr(out res);
  } else if (NextIs_TypeExpr(1)) {
   TypeIdentExpr(out res, true);
  } else if (NextIs_MethodExpr(1)) {
   MethodExpr(out res);
  } else if (la.kind == 1) {
   IdentExpr(out res);
  } else if (la.kind == 62) {
   IndexExpr(out res);
  } else if (la.kind == 42) {
   Get();
   if (NextIs_TypeExpr(1)) {
    TypeIdentExpr(out res, false);
    ((NType)res).Kind = NType.NKind.Attached;
   } else if (StartOf(1)) {
    Expr(out res);
   } else SynErr(83);
   Expect(43);
  } else SynErr(84);
  ReadNextIdents(ref res, true);
 }

 void ConstExpr(out NBase res) {
  res = null;
  switch (la.kind) {
  case 2: {
   Get();
   res = new NConstant(NConstant.NKind.Integer, ParserHelper.ParseInt(t.val));
   break;
  }
  case 3: {
   Get();
   res = new NConstant(NConstant.NKind.Float, ParserHelper.ParseFloat(t.val));
   break;
  }
  case 4: {
   Get();
   res = new NConstant(NConstant.NKind.String, ParserHelper.ParseString(t.val));
   break;
  }
  case 64: {
   Get();
   res = new NConstant(NConstant.NKind.Boolean, ParserHelper.ParseBool(t.val));
   break;
  }
  case 65: {
   Get();
   res = new NConstant(NConstant.NKind.Boolean, ParserHelper.ParseBool(t.val));
   break;
  }
  case 66: {
   Get();
   res = new NConstant(NConstant.NKind.Null, null);
   break;
  }
  default: SynErr(85); break;
  }
 }

 void RelativeExpr(out NBase res) {
  res = null;
  switch (la.kind) {
  case 44: case 45: {
   if (la.kind == 44) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Context);
   break;
  }
  case 46: case 47: {
   if (la.kind == 46) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Self);
   break;
  }
  case 48: case 49: {
   if (la.kind == 48) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Parent);
   break;
  }
  case 50: case 51: {
   if (la.kind == 50) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Element);
   Expect(42);
   Expect(1);
   ((NRelative)res).ElementName = t.val;
   Expect(43);
   break;
  }
  case 52: case 53: {
   if (la.kind == 52) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Resource);
   Expect(42);
   Expect(1);
   ((NRelative)res).ResourceName = t.val;
   Expect(43);
   break;
  }
  case 54: {
   Get();
   res = new NRelative(t.val, null, NRelative.NKind.Reference);
   Expect(42);
   Expect(1);
   ((NRelative)res).ReferenceName = t.val;
   Expect(43);
   break;
  }
  case 55: case 56: {
   if (la.kind == 55) {
    Get();
   } else {
    Get();
   }
   res = new NRelative(t.val, null, NRelative.NKind.Ancestor); NBase type;
   Expect(42);
   TypeExpr(out type);
   ((NRelative)res).AncestorType = (NType)type;
   if (la.kind == 57) {
    Get();
    Expect(2);
    ((NRelative)res).AncestorLevel = (int)ParserHelper.ParseInt(t.val);
   }
   Expect(43);
   break;
  }
  default: SynErr(86); break;
  }
 }

 void TypeOfExpr(out NBase res) {
  Expect(58);
  Expect(42);
  TypeExpr(out res);
  Expect(43);
  ((NType)res).Kind = NType.NKind.TypeOf;
 }

 void TypeNewExpr(out NBase res) {
  res = null; NBase type; NArgs args;
  Expect(61);
  TypeExpr(out type);
  MethodArgsExpr(out args);
  res = new NNew((NType)type, null, args);
 }

 void TypeIdentExpr(out NBase res, bool allowMethod) {
  res = null; NBase ident = null;
  TypeExpr(out res);
  Expect(59);
  ((NType)res).Kind = NType.NKind.Static;
  if (allowMethod && NextIs_MethodExpr(1)) {
   MethodExpr(out ident);
  } else if (la.kind == 1) {
   IdentExpr(out ident);
  } else SynErr(87);
  ((NType)res).Ident = (NIdentBase)ident;
 }

 void MethodExpr(out NBase res) {
  res = null; string name; NArgs args;
  Expect(1);
  name = t.val;
  MethodArgsExpr(out args);
  res = new NMethod(name, null, args);
 }

 void IdentExpr(out NBase res) {
  res = null;
  Expect(1);
  res = new NIdent(t.val, null);
 }

 void IndexExpr(out NBase res) {
  res = null; NArgs args = new NArgs(); NBase arg = null;
  Expect(62);
  Expr(out arg);
  args.Add(arg);
  while (la.kind == 57) {
   Get();
   Expr(out arg);
   args.Add(arg);
  }
  Expect(63);
  res = new NIndex(null, args);
 }

 void ReadNextIdents(ref NBase n, bool allowMethod) {
  NIdentBase cur = n as NIdentBase; NBase next = null;
  while (la.kind == 62) {
   IndexExpr(out next);
   if(cur == null) cur = (NIdentBase)(n = new NExprIdent(n, null));
   cur = cur.Next = (NIdentBase)next;
  }
  while (la.kind == 59) {
   NextIdentExpr(out next, allowMethod);
   if(cur == null) cur = (NIdentBase)(n = new NExprIdent(n, null));
   cur = cur.Next = (NIdentBase)next;
   while (la.kind == 62) {
    IndexExpr(out next);
    cur = cur.Next = (NIdentBase)next;
   }
  }
 }

 void NextIdentExpr(out NBase res, bool allowMethod) {
  res = null;
  Expect(59);
  if (la.kind == 42) {
   Get();
   TypeIdentExpr(out res, false);
   Expect(43);
   ((NType)res).Kind = NType.NKind.Attached;
  } else if (allowMethod && NextIs_MethodExpr(1)) {
   MethodExpr(out res);
  } else if (la.kind == 1) {
   IdentExpr(out res);
  } else SynErr(88);
 }

 void MethodArgsExpr(out NArgs res) {
  res = new NArgs(); NBase arg;
  Expect(42);
  if (StartOf(1)) {
   Expr(out arg);
   res.Add(arg);
   while (la.kind == 57) {
    Get();
    Expr(out arg);
    res.Add(arg);
   }
  }
  Expect(43);
 }

 void Back_Assign(out NBase res) {
  res = null; NBase left; NBase expr;
  Back_AssignLeft(out left);
  Expect(6);
  Expr(out expr);
  res = new NAssign((NIdentBase)left, expr);
 }

 void Back_AssignLeft(out NBase res) {
  res = null;
  if (StartOf(6)) {
   RelativeExpr(out res);
  } else if (la.kind == 1) {
   IdentExpr(out res);
  } else if (la.kind == 62) {
   IndexExpr(out res);
  } else if (la.kind == 42) {
   Get();
   TypeIdentExpr(out res, false);
   ((NType)res).Kind = NType.NKind.Attached;
   Expect(43);
  } else SynErr(89);
  ReadNextIdents(ref res, false);
 }

 void Back_RelativeValueExpr(out NBase res) {
  res = null;
  if (la.kind == 67) {
   Get();
  } else if (la.kind == 68) {
   Get();
  } else SynErr(90);
  res = new NRelative(t.val, null, NRelative.NKind.Value);
 }

 void Execute_RelativeExpr(out NBase res) {
  res = null;
  if (StartOf(6)) {
   RelativeExpr(out res);
  } else if (la.kind == 69) {
   Get();
   res = new NRelative(t.val, null, NRelative.NKind.Parameter);
  } else SynErr(91);
 }

 void Event_RelativeExpr(out NBase res) {
  res = null;
  if (StartOf(6)) {
   RelativeExpr(out res);
  } else if (la.kind == 70) {
   Get();
   res = new NRelative(t.val, null, NRelative.NKind.Sender);
  } else if (la.kind == 71) {
   Get();
   res = new NRelative(t.val, null, NRelative.NKind.Args);
  } else SynErr(92);
 }


 protected override void ParseRoot() {
  DXBinding();
  Expect(0);

 }
 protected override bool[,] GetSet() {
  return new bool[,] {
  {_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
  {_x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_x, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
  {_x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x},
  {_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x}

  };
 }
}
class Errors : ErrorsBase {
 public Errors(IParserErrorHandler errorHandler) : base(errorHandler) { }
 internal static string GetDefaultError(int n) {
  string s;
  switch (n) {
   case 0: s = "EOF expected"; break;
   case 1: s = "Ident expected"; break;
   case 2: s = "Int expected"; break;
   case 3: s = "Float expected"; break;
   case 4: s = "String expected"; break;
   case 5: s = "\";\" expected"; break;
   case 6: s = "\"=\" expected"; break;
   case 7: s = "\"?\" expected"; break;
   case 8: s = "\":\" expected"; break;
   case 9: s = "\"??\" expected"; break;
   case 10: s = "\"||\" expected"; break;
   case 11: s = "\"or\" expected"; break;
   case 12: s = "\"&&\" expected"; break;
   case 13: s = "\"and\" expected"; break;
   case 14: s = "\"|\" expected"; break;
   case 15: s = "\"^\" expected"; break;
   case 16: s = "\"&\" expected"; break;
   case 17: s = "\"!=\" expected"; break;
   case 18: s = "\"ne\" expected"; break;
   case 19: s = "\"==\" expected"; break;
   case 20: s = "\"eq\" expected"; break;
   case 21: s = "\"<\" expected"; break;
   case 22: s = "\"lt\" expected"; break;
   case 23: s = "\">\" expected"; break;
   case 24: s = "\"gt\" expected"; break;
   case 25: s = "\"<=\" expected"; break;
   case 26: s = "\"le\" expected"; break;
   case 27: s = "\">=\" expected"; break;
   case 28: s = "\"ge\" expected"; break;
   case 29: s = "\"is\" expected"; break;
   case 30: s = "\"as\" expected"; break;
   case 31: s = "\"<<\" expected"; break;
   case 32: s = "\"shl\" expected"; break;
   case 33: s = "\">>\" expected"; break;
   case 34: s = "\"shr\" expected"; break;
   case 35: s = "\"+\" expected"; break;
   case 36: s = "\"-\" expected"; break;
   case 37: s = "\"*\" expected"; break;
   case 38: s = "\"/\" expected"; break;
   case 39: s = "\"%\" expected"; break;
   case 40: s = "\"~\" expected"; break;
   case 41: s = "\"!\" expected"; break;
   case 42: s = "\"(\" expected"; break;
   case 43: s = "\")\" expected"; break;
   case 44: s = "\"@c\" expected"; break;
   case 45: s = "\"@DataContext\" expected"; break;
   case 46: s = "\"@s\" expected"; break;
   case 47: s = "\"@Self\" expected"; break;
   case 48: s = "\"@p\" expected"; break;
   case 49: s = "\"@TemplatedParent\" expected"; break;
   case 50: s = "\"@e\" expected"; break;
   case 51: s = "\"@ElementName\" expected"; break;
   case 52: s = "\"@r\" expected"; break;
   case 53: s = "\"@StaticResource\" expected"; break;
   case 54: s = "\"@Reference\" expected"; break;
   case 55: s = "\"@a\" expected"; break;
   case 56: s = "\"@FindAncestor\" expected"; break;
   case 57: s = "\",\" expected"; break;
   case 58: s = "\"typeof\" expected"; break;
   case 59: s = "\".\" expected"; break;
   case 60: s = "\"$\" expected"; break;
   case 61: s = "\"new\" expected"; break;
   case 62: s = "\"[\" expected"; break;
   case 63: s = "\"]\" expected"; break;
   case 64: s = "\"true\" expected"; break;
   case 65: s = "\"false\" expected"; break;
   case 66: s = "\"null\" expected"; break;
   case 67: s = "\"@v\" expected"; break;
   case 68: s = "\"@value\" expected"; break;
   case 69: s = "\"@parameter\" expected"; break;
   case 70: s = "\"@sender\" expected"; break;
   case 71: s = "\"@args\" expected"; break;
   case 72: s = "??? expected"; break;
   case 73: s = "invalid DXBinding"; break;
   case 74: s = "invalid Back_ExprRoot"; break;
   case 75: s = "invalid Assign_Expr"; break;
   case 76: s = "invalid TypeExpr"; break;
   case 77: s = "invalid AtomRootExpr"; break;
   case 78: s = "invalid AtomExpr"; break;
   case 79: s = "invalid AtomExpr"; break;
   case 80: s = "invalid Back_AtomExpr"; break;
   case 81: s = "invalid Execute_AtomExpr"; break;
   case 82: s = "invalid Execute_AtomExpr"; break;
   case 83: s = "invalid Event_AtomExpr"; break;
   case 84: s = "invalid Event_AtomExpr"; break;
   case 85: s = "invalid ConstExpr"; break;
   case 86: s = "invalid RelativeExpr"; break;
   case 87: s = "invalid TypeIdentExpr"; break;
   case 88: s = "invalid NextIdentExpr"; break;
   case 89: s = "invalid Back_AssignLeft"; break;
   case 90: s = "invalid Back_RelativeValueExpr"; break;
   case 91: s = "invalid Execute_RelativeExpr"; break;
   case 92: s = "invalid Event_RelativeExpr"; break;

   default: s = "error " + n; break;
  }
  return s;
 }
 protected override string GetError(int n) {
  return GetDefaultError(n);
 }
}}