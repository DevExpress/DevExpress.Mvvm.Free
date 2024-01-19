using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace DevExpress.DXBinding.Native {
    static class ParserHelper {
        static Stream StringToStream(string s) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static NRoot GetSyntaxTree(string input, ParserMode parserMode, IParserErrorHandler errorHandler) {
            Scanner s = new Scanner(StringToStream(input));
            Parser p = new Parser(s, errorHandler) { Mode = parserMode };
            p.Parse();
            return errorHandler.HasError ? null : p.Root;
        }

        static string Unescape(string value) {
            var escapes = new Dictionary<string, string> {
                { @"\`", "`" },
                { "\\\"", "\"" },
                { @"\", @"\" },
                { @"\0", "\0" },
                { @"\a", "\a" },
                { @"\b", "\b" },
                { @"\f", "\f" },
                { @"\n", "\n" },
                { @"\r", "\r" },
                { @"\t", "\t" },
                { @"\v", "\v" },
            };
            foreach(var e in escapes) {
                value = value.Replace(e.Key, e.Value);
            }
            return value;
        }

        public static object ParseString(string value) {
            if(value.StartsWith("`"))
                return Unescape(value.Substring(1, value.Length - 2));
            return value;
        }
        public static object ParseBool(string value) {
            return bool.Parse(value);
        }
        public static object ParseFloat(string value) {
            value = value.ToLower();
            if(value.EndsWith("f")) return float.Parse(RemoveLastChars(value, 1), CultureInfo.InvariantCulture);
            if(value.EndsWith("d")) return double.Parse(RemoveLastChars(value, 1), CultureInfo.InvariantCulture);
            if(value.EndsWith("m")) return decimal.Parse(RemoveLastChars(value, 1), CultureInfo.InvariantCulture);
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
        public static object ParseInt(string value) {
            value = value.ToLower();
            if(value.StartsWith("0x"))
                value = value.Substring(2);
            if(value.EndsWith("ul") || value.EndsWith("lu")) return ulong.Parse(RemoveLastChars(value, 2), CultureInfo.InvariantCulture);
            if(value.EndsWith("l")) return long.Parse(RemoveLastChars(value, 1), CultureInfo.InvariantCulture);
            if(value.EndsWith("u")) return uint.Parse(RemoveLastChars(value, 1), CultureInfo.InvariantCulture);
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        public static string RemoveFirstChar(string value) {
            return value.Substring(1, value.Length - 1);
        }
        static string RemoveLastChars(string value, int count) {
            return value.Substring(0, value.Length - count);
        }

        public static bool CastNumericTypes(ref Expression left, ref Expression right) {
            Type _left = left.Type;
            Type _right = right.Type;
            var res = CastNumericTypes(ref _left, ref _right);
            if(_left != left.Type) left = Expression.Convert(left, _left);
            if(_right != right.Type) right = Expression.Convert(right, _right);
            return res;
        }
        public static bool CastNumericTypes(ref Type left, ref Type right) {
            var _left = Nullable.GetUnderlyingType(left) ?? left;
            var _right = Nullable.GetUnderlyingType(right) ?? right;
            if(_left.IsEnum || _right.IsEnum) return false;
            TypeCode leftTypeCode = Type.GetTypeCode(_left);
            TypeCode rightTypeCode = Type.GetTypeCode(_right);
            Func<TypeCode, bool> isNumericType = x => (int)x >= 5 && (int)x <= 15;
            if(!isNumericType(leftTypeCode) || !isNumericType(rightTypeCode)) return false;

            var isNullable = _left != left || _right != right;
            Func<Type, Type> createNullableTypeIfNeeded =
                x => isNullable ? typeof(Nullable<>).MakeGenericType(x) : x;
            if((int)leftTypeCode < (int)rightTypeCode) {
                left = createNullableTypeIfNeeded(_right);
                right = createNullableTypeIfNeeded(_right);
                return true;
            }
            if((int)leftTypeCode > (int)rightTypeCode) {
                left = createNullableTypeIfNeeded(_left);
                right = createNullableTypeIfNeeded(_left);
                return true;
            }
            left = createNullableTypeIfNeeded(_left);
            right = createNullableTypeIfNeeded(_right);
            return true;
        }
        static bool CanCastValueType(TypeCode from, TypeCode to) {
            if(from == to) return true;
            switch(from) {
                case TypeCode.SByte:
                    switch(to) {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch(to) {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch(to) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch(to) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch(to) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch(to) {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch(to) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch(to) {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch(to) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default: break;
            }
            return false;
        }
    }

    abstract class ParserBase {
        protected const bool _T = true;
        protected const bool _x = false;
        protected const int minErrDist = 2;
        protected static bool[,] set;

        public ErrorsBase Errors { get; private set; }
        protected Scanner Scanner { get; private set; }
        protected Token t;
        protected Token la;
        int errDist = minErrDist;
        public ParserBase(Scanner scanner, IParserErrorHandler errorHandler) {
            Errors = CreateErrorHandler(errorHandler);
            Scanner = scanner;
            set = GetSet();
        }
        public void Parse() {
            la = new Token();
            la.val = "";
            Get();
            ParseRoot();
        }
        protected abstract ErrorsBase CreateErrorHandler(IParserErrorHandler errorHandler);

        protected bool TokenEquals(int n, params string[] value) {
            return TokenEquals(n, x => value.Contains(x.val));
        }
        protected bool TokenEquals(int n, params int[] value) {
            return TokenEquals(n, x => value.Contains(x.kind));
        }
        bool TokenEquals(int n, Func<Token, bool> check) {
            if(n == 0) return check(t);
            Token curT = la;
            Scanner.ResetPeek();
            for(int i = 1; i < n; i++)
                curT = Scanner.Peek();
            Scanner.ResetPeek();
            return check(curT);
        }
        protected bool TokenExists(int n, params string[] value) {
            return TokenExists(n, x => value.Contains(x.val));
        }
        protected bool TokenExists(int n, params int[] value) {
            return TokenExists(n, x => value.Contains(x.kind));
        }
        bool TokenExists(int n, Func<Token, bool> check) {
            Token curT = t;
            Scanner.ResetPeek();
            for(int i = 0; i < n; i++)
                curT = Scanner.Peek();
            do {
                if(check(curT)) return true;
                curT = Scanner.Peek();
            } while(!IsEOF(curT.kind));
            Scanner.ResetPeek();
            return false;
        }

        protected abstract void ParseRoot();
        protected abstract bool[,] GetSet();
        protected abstract int GetMaxTokenKind();
        protected abstract void Pragmas();
        protected abstract bool IsEOF(int n);

        protected void Get() {
            for(;;) {
                t = la;
                la = Scanner.Scan();
                if(la.kind <= GetMaxTokenKind()) { ++errDist; break; }
                la = t;
            }
        }
        protected void Expect(int n) {
            if(la.kind == n) Get(); else { SynErr(n); }
        }
        protected bool StartOf(int s) {
            return set[s, la.kind];
        }
        protected void ExpectWeak(int n, int follow) {
            if(la.kind == n) Get();
            else {
                SynErr(n);
                while(!StartOf(follow)) Get();
            }
        }
        protected bool WeakSeparator(int n, int syFol, int repFol) {
            int kind = la.kind;
            if(kind == n) { Get(); return true; } else if(StartOf(repFol)) { return false; } else {
                SynErr(n);
                while(!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }
        protected void SynErr(int n) {
            if(errDist >= minErrDist) Errors.SynErr(la.line, la.col, n);
            errDist = 0;
        }
        protected void SemErr(string msg) {
            if(errDist >= minErrDist) Errors.SemErr(t.line, t.col, msg);
            errDist = 0;
        }
    }
    abstract class ErrorsBase {
        readonly IParserErrorHandler errorHandler;
        public ErrorsBase(IParserErrorHandler errorHandler) {
            this.errorHandler = errorHandler;
        }
        protected abstract string GetError(int n);

        public void SynErr(int line, int col, int n) { WriteLine(col, GetError(n)); }
        public void SemErr(int line, int col, string s) { WriteLine(col, s); }
        public void SemErr(string s) { WriteLine(-1, s); }
        public void Warning(int line, int col, string s) { WriteLine(col, s); }
        public void Warning(string s) { WriteLine(-1, s); }
        void WriteLine(int col, string s) {
            errorHandler.Error(col, s);
        }
    }
    class FatalError : Exception {
        public FatalError(string m) : base(m) { }
    }

    class Token {
        public int kind;
        public int pos;
        public int charPos;
        public int col;
        public int line;
        public string val;
        public Token next;
    }
    abstract class ScannerBase {
        protected const char EOL = '\n';
        protected const int eofSym = 0;

        public Buffer buffer;

        protected Token t;
        protected int ch;
        protected int pos;
        protected int charPos;
        protected int col;
        protected int line;
        protected int oldEols;
        protected static readonly Hashtable start;

        protected Token tokens;
        protected Token pt;

        protected char[] tval = new char[128];
        protected int tlen;

        static ScannerBase() {
            start = new Hashtable(128);
        }
        public ScannerBase(string fileName) {
            try {
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                buffer = new Buffer(stream, false);
                Init();
            } catch(IOException) {
                throw new FatalError("Cannot open file " + fileName);
            }
        }
        public ScannerBase(Stream s) {
            buffer = new Buffer(s, true);
            Init();
        }

        protected void Init() {
            pos = -1; line = 1; col = 0; charPos = -1;
            oldEols = 0;
            NextCh();
            if(ch == 0xEF) {
                NextCh(); int ch1 = ch;
                NextCh(); int ch2 = ch;
                if(ch1 != 0xBB || ch2 != 0xBF) {
                    throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
                }
                buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
                NextCh();
            }
            pt = tokens = new Token();
        }
        protected void NextCh() {
            if(oldEols > 0) { ch = EOL; oldEols--; } else {
                pos = buffer.Pos;
                ch = buffer.Read(); col++; charPos++;
                if(ch == '\r' && buffer.Peek() != '\n') ch = EOL;
                if(ch == EOL) { line++; col = 0; }
            }
            Casing1();
        }
        protected void AddCh() {
            if(tlen >= tval.Length) {
                char[] newBuf = new char[2 * tval.Length];
                Array.Copy(tval, 0, newBuf, 0, tval.Length);
                tval = newBuf;
            }
            if(ch != Buffer.EOF) {
                Casing2();
                NextCh();
            }
        }
        protected abstract void Casing1();
        protected abstract void Casing2();
        protected abstract void CheckLiteral();

        protected abstract int GetMaxT();
        protected abstract Token NextToken();

        protected void SetScannerBehindT() {
            buffer.Pos = t.pos;
            NextCh();
            line = t.line; col = t.col; charPos = t.charPos;
            for(int i = 0; i < tlen; i++) NextCh();
        }
        public Token Scan() {
            if(tokens.next == null) {
                return NextToken();
            } else {
                pt = tokens = tokens.next;
                return tokens;
            }
        }
        public Token Peek() {
            do {
                if(pt.next == null) {
                    pt.next = NextToken();
                }
                pt = pt.next;
            } while(pt.kind > GetMaxT());
            return pt;
        }
        public void ResetPeek() { pt = tokens; }
        public Token PeekWithPragmas() {
            if(pt.next == null) {
                pt.next = NextToken();
            }
            pt = pt.next;
            return pt;
        }
    }

    class Buffer {
        public const int EOF = char.MaxValue + 1;
        const int MIN_BUFFER_LENGTH = 1024;
        const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64;
        byte[] buf;
        int bufStart;
        int bufLen;
        int fileLen;
        int bufPos;
        Stream stream;
        bool isUserStream;

        public Buffer(Stream s, bool isUserStream) {
            stream = s; this.isUserStream = isUserStream;

            if(stream.CanSeek) {
                fileLen = (int)stream.Length;
                bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
                bufStart = Int32.MaxValue;
            } else {
                fileLen = bufLen = bufStart = 0;
            }

            buf = new byte[(bufLen > 0) ? bufLen : MIN_BUFFER_LENGTH];
            if(fileLen > 0) Pos = 0;
            else bufPos = 0;
            if(bufLen == fileLen && stream.CanSeek) Close();
        }
        protected Buffer(Buffer b) {
            buf = b.buf;
            bufStart = b.bufStart;
            bufLen = b.bufLen;
            fileLen = b.fileLen;
            bufPos = b.bufPos;
            stream = b.stream;
            b.stream = null;
            isUserStream = b.isUserStream;
        }
        ~Buffer() { Close(); }

        protected void Close() {
            if(!isUserStream && stream != null) {
                stream.Close();
                stream = null;
            }
        }
        public virtual int Read() {
            if(bufPos < bufLen) {
                return buf[bufPos++];
            } else if(Pos < fileLen) {
                Pos = Pos;
                return buf[bufPos++];
            } else if(stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
                return buf[bufPos++];
            } else {
                return EOF;
            }
        }
        public int Peek() {
            int curPos = Pos;
            int ch = Read();
            Pos = curPos;
            return ch;
        }
        public string GetString(int beg, int end) {
            int len = 0;
            char[] buf = new char[end - beg];
            int oldPos = Pos;
            Pos = beg;
            while(Pos < end) buf[len++] = (char)Read();
            Pos = oldPos;
            return new String(buf, 0, len);
        }
        public int Pos {
            get { return bufPos + bufStart; }
            set {
                if(value >= fileLen && stream != null && !stream.CanSeek) {
                    while(value >= fileLen && ReadNextStreamChunk() > 0) ;
                }

                if(value < 0 || value > fileLen) {
                    throw new FatalError("buffer out of bounds access, position: " + value);
                }

                if(value >= bufStart && value < bufStart + bufLen) {
                    bufPos = value - bufStart;
                } else if(stream != null) {
                    stream.Seek(value, SeekOrigin.Begin);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    bufStart = value; bufPos = 0;
                } else {
                    bufPos = fileLen - bufStart;
                }
            }
        }
        private int ReadNextStreamChunk() {
            int free = buf.Length - bufLen;
            if(free == 0) {
                byte[] newBuf = new byte[bufLen * 2];
                Array.Copy(buf, newBuf, bufLen);
                buf = newBuf;
                free = bufLen;
            }
            int read = stream.Read(buf, bufLen, free);
            if(read > 0) {
                fileLen = bufLen = (bufLen + read);
                return read;
            }
            return 0;
        }
    }
    class UTF8Buffer : Buffer {
        public UTF8Buffer(Buffer b) : base(b) { }
        public override int Read() {
            int ch;
            do {
                ch = base.Read();
            } while((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
            if(ch < 128 || ch == EOF) {
            } else if((ch & 0xF0) == 0xF0) {
                int c1 = ch & 0x07; ch = base.Read();
                int c2 = ch & 0x3F; ch = base.Read();
                int c3 = ch & 0x3F; ch = base.Read();
                int c4 = ch & 0x3F;
                ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
            } else if((ch & 0xE0) == 0xE0) {
                int c1 = ch & 0x0F; ch = base.Read();
                int c2 = ch & 0x3F; ch = base.Read();
                int c3 = ch & 0x3F;
                ch = (((c1 << 6) | c2) << 6) | c3;
            } else if((ch & 0xC0) == 0xC0) {
                int c1 = ch & 0x1F; ch = base.Read();
                int c2 = ch & 0x3F;
                ch = (c1 << 6) | c2;
            }
            return ch;
        }
    }
}