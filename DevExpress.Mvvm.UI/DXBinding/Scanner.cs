
using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Text;
namespace DevExpress.DXBinding.Native {
class Scanner : ScannerBase {
 const int maxT = 72;
 const int noSym = 72;

  static Scanner() {
  for (int i = 65; i <= 90; ++i) start[i] = 1;
  for (int i = 95; i <= 95; ++i) start[i] = 1;
  for (int i = 97; i <= 122; ++i) start[i] = 1;
  for (int i = 49; i <= 57; ++i) start[i] = 16;
  start[48] = 17;
  start[46] = 148;
  start[96] = 14;
  start[59] = 27;
  start[61] = 149;
  start[63] = 150;
  start[58] = 28;
  start[124] = 151;
  start[38] = 152;
  start[94] = 32;
  start[33] = 153;
  start[60] = 154;
  start[62] = 155;
  start[43] = 39;
  start[45] = 40;
  start[42] = 41;
  start[47] = 42;
  start[37] = 43;
  start[126] = 44;
  start[40] = 45;
  start[41] = 46;
  start[64] = 156;
  start[44] = 124;
  start[36] = 125;
  start[91] = 126;
  start[93] = 127;
  start[Buffer.EOF] = -1;

  }
        public Scanner(string fileName) : base(fileName) { }
        public Scanner(Stream s) : base(s) { }

        protected override void Casing1() {

  }
        protected override void Casing2() {
   tval[tlen++] = (char) ch;
  }



  protected override void CheckLiteral() {
  switch (t.val) {
   case "or": t.kind = 11; break;
   case "and": t.kind = 13; break;
   case "ne": t.kind = 18; break;
   case "eq": t.kind = 20; break;
   case "lt": t.kind = 22; break;
   case "gt": t.kind = 24; break;
   case "le": t.kind = 26; break;
   case "ge": t.kind = 28; break;
   case "is": t.kind = 29; break;
   case "as": t.kind = 30; break;
   case "shl": t.kind = 32; break;
   case "shr": t.kind = 34; break;
   case "typeof": t.kind = 58; break;
   case "new": t.kind = 61; break;
   case "true": t.kind = 64; break;
   case "false": t.kind = 65; break;
   case "null": t.kind = 66; break;
   default: break;
  }
  }
  protected override int GetMaxT() {
   return maxT;
  }
        protected override Token NextToken() {
   while (ch == ' ' ||
   ch >= 9 && ch <= 10 || ch == 13
   ) NextCh();

   int recKind = noSym;
   int recEnd = pos;
   t = new Token();
   t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
   int state;
   if (start.ContainsKey(ch)) { state = (int) start[ch]; }
   else { state = 0; }
   tlen = 0; AddCh();

   switch (state) {
    case -1: { t.kind = eofSym; break; }
    case 0: {
     if (recKind != noSym) {
      tlen = recEnd - t.pos;
      SetScannerBehindT();
     }
     t.kind = recKind; break;
    }
   case 1:
    recEnd = pos; recKind = 1;
    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
    else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
   case 2:
    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 3;}
    else {goto case 0;}
   case 3:
    recEnd = pos; recKind = 2;
    if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 3;}
    else if (ch == 'U') {AddCh(); goto case 22;}
    else if (ch == 'u') {AddCh(); goto case 23;}
    else if (ch == 'L') {AddCh(); goto case 24;}
    else if (ch == 'l') {AddCh(); goto case 25;}
    else {t.kind = 2; break;}
   case 4:
    {t.kind = 2; break;}
   case 5:
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
    else {goto case 0;}
   case 6:
    recEnd = pos; recKind = 3;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
    else if (ch == 'D' || ch == 'F' || ch == 'M' || ch == 'd' || ch == 'f' || ch == 'm') {AddCh(); goto case 13;}
    else if (ch == 'E' || ch == 'e') {AddCh(); goto case 7;}
    else {t.kind = 3; break;}
   case 7:
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
    else if (ch == '+' || ch == '-') {AddCh(); goto case 8;}
    else {goto case 0;}
   case 8:
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
    else {goto case 0;}
   case 9:
    recEnd = pos; recKind = 3;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 9;}
    else if (ch == 'D' || ch == 'F' || ch == 'M' || ch == 'd' || ch == 'f' || ch == 'm') {AddCh(); goto case 13;}
    else {t.kind = 3; break;}
   case 10:
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
    else if (ch == '+' || ch == '-') {AddCh(); goto case 11;}
    else {goto case 0;}
   case 11:
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
    else {goto case 0;}
   case 12:
    recEnd = pos; recKind = 3;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
    else if (ch == 'D' || ch == 'F' || ch == 'M' || ch == 'd' || ch == 'f' || ch == 'm') {AddCh(); goto case 13;}
    else {t.kind = 3; break;}
   case 13:
    {t.kind = 3; break;}
   case 14:
    if (ch <= 9 || ch >= 11 && ch <= '[' || ch >= ']' && ch <= '_' || ch >= 'a' && ch <= 65535) {AddCh(); goto case 14;}
    else if (ch == '`') {AddCh(); goto case 15;}
    else if (ch == 92) {AddCh(); goto case 26;}
    else {goto case 0;}
   case 15:
    {t.kind = 4; break;}
   case 16:
    recEnd = pos; recKind = 2;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 16;}
    else if (ch == 'U') {AddCh(); goto case 18;}
    else if (ch == 'u') {AddCh(); goto case 19;}
    else if (ch == 'L') {AddCh(); goto case 20;}
    else if (ch == 'l') {AddCh(); goto case 21;}
    else if (ch == 'D' || ch == 'F' || ch == 'M' || ch == 'd' || ch == 'f' || ch == 'm') {AddCh(); goto case 13;}
    else if (ch == '.') {AddCh(); goto case 5;}
    else if (ch == 'E' || ch == 'e') {AddCh(); goto case 10;}
    else {t.kind = 2; break;}
   case 17:
    recEnd = pos; recKind = 2;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 16;}
    else if (ch == 'U') {AddCh(); goto case 18;}
    else if (ch == 'u') {AddCh(); goto case 19;}
    else if (ch == 'L') {AddCh(); goto case 20;}
    else if (ch == 'l') {AddCh(); goto case 21;}
    else if (ch == 'X' || ch == 'x') {AddCh(); goto case 2;}
    else if (ch == 'D' || ch == 'F' || ch == 'M' || ch == 'd' || ch == 'f' || ch == 'm') {AddCh(); goto case 13;}
    else if (ch == '.') {AddCh(); goto case 5;}
    else if (ch == 'E' || ch == 'e') {AddCh(); goto case 10;}
    else {t.kind = 2; break;}
   case 18:
    recEnd = pos; recKind = 2;
    if (ch == 'L' || ch == 'l') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 19:
    recEnd = pos; recKind = 2;
    if (ch == 'L' || ch == 'l') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 20:
    recEnd = pos; recKind = 2;
    if (ch == 'U' || ch == 'u') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 21:
    recEnd = pos; recKind = 2;
    if (ch == 'U' || ch == 'u') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 22:
    recEnd = pos; recKind = 2;
    if (ch == 'L' || ch == 'l') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 23:
    recEnd = pos; recKind = 2;
    if (ch == 'L' || ch == 'l') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 24:
    recEnd = pos; recKind = 2;
    if (ch == 'U' || ch == 'u') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 25:
    recEnd = pos; recKind = 2;
    if (ch == 'U' || ch == 'u') {AddCh(); goto case 4;}
    else {t.kind = 2; break;}
   case 26:
    if (ch == '"' || ch == 39 || ch == '0' || ch == 92 || ch >= '`' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v') {AddCh(); goto case 14;}
    else {goto case 0;}
   case 27:
    {t.kind = 5; break;}
   case 28:
    {t.kind = 8; break;}
   case 29:
    {t.kind = 9; break;}
   case 30:
    {t.kind = 10; break;}
   case 31:
    {t.kind = 12; break;}
   case 32:
    {t.kind = 15; break;}
   case 33:
    {t.kind = 17; break;}
   case 34:
    {t.kind = 19; break;}
   case 35:
    {t.kind = 25; break;}
   case 36:
    {t.kind = 27; break;}
   case 37:
    {t.kind = 31; break;}
   case 38:
    {t.kind = 33; break;}
   case 39:
    {t.kind = 35; break;}
   case 40:
    {t.kind = 36; break;}
   case 41:
    {t.kind = 37; break;}
   case 42:
    {t.kind = 38; break;}
   case 43:
    {t.kind = 39; break;}
   case 44:
    {t.kind = 40; break;}
   case 45:
    {t.kind = 42; break;}
   case 46:
    {t.kind = 43; break;}
   case 47:
    {t.kind = 44; break;}
   case 48:
    if (ch == 'a') {AddCh(); goto case 49;}
    else {goto case 0;}
   case 49:
    if (ch == 't') {AddCh(); goto case 50;}
    else {goto case 0;}
   case 50:
    if (ch == 'a') {AddCh(); goto case 51;}
    else {goto case 0;}
   case 51:
    if (ch == 'C') {AddCh(); goto case 52;}
    else {goto case 0;}
   case 52:
    if (ch == 'o') {AddCh(); goto case 53;}
    else {goto case 0;}
   case 53:
    if (ch == 'n') {AddCh(); goto case 54;}
    else {goto case 0;}
   case 54:
    if (ch == 't') {AddCh(); goto case 55;}
    else {goto case 0;}
   case 55:
    if (ch == 'e') {AddCh(); goto case 56;}
    else {goto case 0;}
   case 56:
    if (ch == 'x') {AddCh(); goto case 57;}
    else {goto case 0;}
   case 57:
    if (ch == 't') {AddCh(); goto case 58;}
    else {goto case 0;}
   case 58:
    {t.kind = 45; break;}
   case 59:
    if (ch == 'l') {AddCh(); goto case 60;}
    else {goto case 0;}
   case 60:
    if (ch == 'f') {AddCh(); goto case 61;}
    else {goto case 0;}
   case 61:
    {t.kind = 47; break;}
   case 62:
    if (ch == 'e') {AddCh(); goto case 63;}
    else {goto case 0;}
   case 63:
    if (ch == 'm') {AddCh(); goto case 64;}
    else {goto case 0;}
   case 64:
    if (ch == 'p') {AddCh(); goto case 65;}
    else {goto case 0;}
   case 65:
    if (ch == 'l') {AddCh(); goto case 66;}
    else {goto case 0;}
   case 66:
    if (ch == 'a') {AddCh(); goto case 67;}
    else {goto case 0;}
   case 67:
    if (ch == 't') {AddCh(); goto case 68;}
    else {goto case 0;}
   case 68:
    if (ch == 'e') {AddCh(); goto case 69;}
    else {goto case 0;}
   case 69:
    if (ch == 'd') {AddCh(); goto case 70;}
    else {goto case 0;}
   case 70:
    if (ch == 'P') {AddCh(); goto case 71;}
    else {goto case 0;}
   case 71:
    if (ch == 'a') {AddCh(); goto case 72;}
    else {goto case 0;}
   case 72:
    if (ch == 'r') {AddCh(); goto case 73;}
    else {goto case 0;}
   case 73:
    if (ch == 'e') {AddCh(); goto case 74;}
    else {goto case 0;}
   case 74:
    if (ch == 'n') {AddCh(); goto case 75;}
    else {goto case 0;}
   case 75:
    if (ch == 't') {AddCh(); goto case 76;}
    else {goto case 0;}
   case 76:
    {t.kind = 49; break;}
   case 77:
    {t.kind = 50; break;}
   case 78:
    if (ch == 'l') {AddCh(); goto case 79;}
    else {goto case 0;}
   case 79:
    if (ch == 'e') {AddCh(); goto case 80;}
    else {goto case 0;}
   case 80:
    if (ch == 'm') {AddCh(); goto case 81;}
    else {goto case 0;}
   case 81:
    if (ch == 'e') {AddCh(); goto case 82;}
    else {goto case 0;}
   case 82:
    if (ch == 'n') {AddCh(); goto case 83;}
    else {goto case 0;}
   case 83:
    if (ch == 't') {AddCh(); goto case 84;}
    else {goto case 0;}
   case 84:
    if (ch == 'N') {AddCh(); goto case 85;}
    else {goto case 0;}
   case 85:
    if (ch == 'a') {AddCh(); goto case 86;}
    else {goto case 0;}
   case 86:
    if (ch == 'm') {AddCh(); goto case 87;}
    else {goto case 0;}
   case 87:
    if (ch == 'e') {AddCh(); goto case 88;}
    else {goto case 0;}
   case 88:
    {t.kind = 51; break;}
   case 89:
    {t.kind = 52; break;}
   case 90:
    if (ch == 'a') {AddCh(); goto case 91;}
    else {goto case 0;}
   case 91:
    if (ch == 't') {AddCh(); goto case 92;}
    else {goto case 0;}
   case 92:
    if (ch == 'i') {AddCh(); goto case 93;}
    else {goto case 0;}
   case 93:
    if (ch == 'c') {AddCh(); goto case 94;}
    else {goto case 0;}
   case 94:
    if (ch == 'R') {AddCh(); goto case 95;}
    else {goto case 0;}
   case 95:
    if (ch == 'e') {AddCh(); goto case 96;}
    else {goto case 0;}
   case 96:
    if (ch == 's') {AddCh(); goto case 97;}
    else {goto case 0;}
   case 97:
    if (ch == 'o') {AddCh(); goto case 98;}
    else {goto case 0;}
   case 98:
    if (ch == 'u') {AddCh(); goto case 99;}
    else {goto case 0;}
   case 99:
    if (ch == 'r') {AddCh(); goto case 100;}
    else {goto case 0;}
   case 100:
    if (ch == 'c') {AddCh(); goto case 101;}
    else {goto case 0;}
   case 101:
    if (ch == 'e') {AddCh(); goto case 102;}
    else {goto case 0;}
   case 102:
    {t.kind = 53; break;}
   case 103:
    if (ch == 'e') {AddCh(); goto case 104;}
    else {goto case 0;}
   case 104:
    if (ch == 'f') {AddCh(); goto case 105;}
    else {goto case 0;}
   case 105:
    if (ch == 'e') {AddCh(); goto case 106;}
    else {goto case 0;}
   case 106:
    if (ch == 'r') {AddCh(); goto case 107;}
    else {goto case 0;}
   case 107:
    if (ch == 'e') {AddCh(); goto case 108;}
    else {goto case 0;}
   case 108:
    if (ch == 'n') {AddCh(); goto case 109;}
    else {goto case 0;}
   case 109:
    if (ch == 'c') {AddCh(); goto case 110;}
    else {goto case 0;}
   case 110:
    if (ch == 'e') {AddCh(); goto case 111;}
    else {goto case 0;}
   case 111:
    {t.kind = 54; break;}
   case 112:
    if (ch == 'i') {AddCh(); goto case 113;}
    else {goto case 0;}
   case 113:
    if (ch == 'n') {AddCh(); goto case 114;}
    else {goto case 0;}
   case 114:
    if (ch == 'd') {AddCh(); goto case 115;}
    else {goto case 0;}
   case 115:
    if (ch == 'A') {AddCh(); goto case 116;}
    else {goto case 0;}
   case 116:
    if (ch == 'n') {AddCh(); goto case 117;}
    else {goto case 0;}
   case 117:
    if (ch == 'c') {AddCh(); goto case 118;}
    else {goto case 0;}
   case 118:
    if (ch == 'e') {AddCh(); goto case 119;}
    else {goto case 0;}
   case 119:
    if (ch == 's') {AddCh(); goto case 120;}
    else {goto case 0;}
   case 120:
    if (ch == 't') {AddCh(); goto case 121;}
    else {goto case 0;}
   case 121:
    if (ch == 'o') {AddCh(); goto case 122;}
    else {goto case 0;}
   case 122:
    if (ch == 'r') {AddCh(); goto case 123;}
    else {goto case 0;}
   case 123:
    {t.kind = 56; break;}
   case 124:
    {t.kind = 57; break;}
   case 125:
    {t.kind = 60; break;}
   case 126:
    {t.kind = 62; break;}
   case 127:
    {t.kind = 63; break;}
   case 128:
    if (ch == 'l') {AddCh(); goto case 129;}
    else {goto case 0;}
   case 129:
    if (ch == 'u') {AddCh(); goto case 130;}
    else {goto case 0;}
   case 130:
    if (ch == 'e') {AddCh(); goto case 131;}
    else {goto case 0;}
   case 131:
    {t.kind = 68; break;}
   case 132:
    if (ch == 'r') {AddCh(); goto case 133;}
    else {goto case 0;}
   case 133:
    if (ch == 'a') {AddCh(); goto case 134;}
    else {goto case 0;}
   case 134:
    if (ch == 'm') {AddCh(); goto case 135;}
    else {goto case 0;}
   case 135:
    if (ch == 'e') {AddCh(); goto case 136;}
    else {goto case 0;}
   case 136:
    if (ch == 't') {AddCh(); goto case 137;}
    else {goto case 0;}
   case 137:
    if (ch == 'e') {AddCh(); goto case 138;}
    else {goto case 0;}
   case 138:
    if (ch == 'r') {AddCh(); goto case 139;}
    else {goto case 0;}
   case 139:
    {t.kind = 69; break;}
   case 140:
    if (ch == 'n') {AddCh(); goto case 141;}
    else {goto case 0;}
   case 141:
    if (ch == 'd') {AddCh(); goto case 142;}
    else {goto case 0;}
   case 142:
    if (ch == 'e') {AddCh(); goto case 143;}
    else {goto case 0;}
   case 143:
    if (ch == 'r') {AddCh(); goto case 144;}
    else {goto case 0;}
   case 144:
    {t.kind = 70; break;}
   case 145:
    if (ch == 'g') {AddCh(); goto case 146;}
    else {goto case 0;}
   case 146:
    if (ch == 's') {AddCh(); goto case 147;}
    else {goto case 0;}
   case 147:
    {t.kind = 71; break;}
   case 148:
    recEnd = pos; recKind = 59;
    if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
    else {t.kind = 59; break;}
   case 149:
    recEnd = pos; recKind = 6;
    if (ch == '=') {AddCh(); goto case 34;}
    else {t.kind = 6; break;}
   case 150:
    recEnd = pos; recKind = 7;
    if (ch == '?') {AddCh(); goto case 29;}
    else {t.kind = 7; break;}
   case 151:
    recEnd = pos; recKind = 14;
    if (ch == '|') {AddCh(); goto case 30;}
    else {t.kind = 14; break;}
   case 152:
    recEnd = pos; recKind = 16;
    if (ch == '&') {AddCh(); goto case 31;}
    else {t.kind = 16; break;}
   case 153:
    recEnd = pos; recKind = 41;
    if (ch == '=') {AddCh(); goto case 33;}
    else {t.kind = 41; break;}
   case 154:
    recEnd = pos; recKind = 21;
    if (ch == '=') {AddCh(); goto case 35;}
    else if (ch == '<') {AddCh(); goto case 37;}
    else {t.kind = 21; break;}
   case 155:
    recEnd = pos; recKind = 23;
    if (ch == '=') {AddCh(); goto case 36;}
    else if (ch == '>') {AddCh(); goto case 38;}
    else {t.kind = 23; break;}
   case 156:
    if (ch == 'c') {AddCh(); goto case 47;}
    else if (ch == 'D') {AddCh(); goto case 48;}
    else if (ch == 's') {AddCh(); goto case 157;}
    else if (ch == 'S') {AddCh(); goto case 158;}
    else if (ch == 'p') {AddCh(); goto case 159;}
    else if (ch == 'T') {AddCh(); goto case 62;}
    else if (ch == 'e') {AddCh(); goto case 77;}
    else if (ch == 'E') {AddCh(); goto case 78;}
    else if (ch == 'r') {AddCh(); goto case 89;}
    else if (ch == 'R') {AddCh(); goto case 103;}
    else if (ch == 'a') {AddCh(); goto case 160;}
    else if (ch == 'F') {AddCh(); goto case 112;}
    else if (ch == 'v') {AddCh(); goto case 161;}
    else {goto case 0;}
   case 157:
    recEnd = pos; recKind = 46;
    if (ch == 'e') {AddCh(); goto case 140;}
    else {t.kind = 46; break;}
   case 158:
    if (ch == 'e') {AddCh(); goto case 59;}
    else if (ch == 't') {AddCh(); goto case 90;}
    else {goto case 0;}
   case 159:
    recEnd = pos; recKind = 48;
    if (ch == 'a') {AddCh(); goto case 132;}
    else {t.kind = 48; break;}
   case 160:
    recEnd = pos; recKind = 55;
    if (ch == 'r') {AddCh(); goto case 145;}
    else {t.kind = 55; break;}
   case 161:
    recEnd = pos; recKind = 67;
    if (ch == 'a') {AddCh(); goto case 128;}
    else {t.kind = 67; break;}

   }
   var tvalBytes = tval.Select(x => Convert.ToByte(x)).ToArray();
   t.val = Encoding.UTF8.GetString(tvalBytes, 0, tlen);
   return t;
 }
}
}