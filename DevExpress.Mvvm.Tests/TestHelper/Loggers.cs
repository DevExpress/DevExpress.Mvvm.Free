using Microsoft.Silverlight.Testing.Harness;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Browser;

namespace DevExpress {
    abstract class TestLogger : LogProvider {
        int totalCount, passedCount;
        bool isFirstMessage = true;

        public TestLogger() {
        }

        protected abstract void OutputString(string text);

        public override void Process(LogMessage logMessage) {
            if(isFirstMessage) {
                isFirstMessage = false;
                ProcessStartMessage();
            } else
                if(logMessage.HasDecorator(LogDecorator.TestOutcome))
                    ProcessResultMessage(logMessage);
        }
        internal void ProcessEndMessage() {
            InvokeMethod("End", "", totalCount.ToString(), passedCount.ToString());
        }
        static string EscapeString(string p) {
            p = p.Replace("'", "\"");
            p = p.Replace("\r\n", "<br/>");
            return p;
        }
        string FormatException(Exception exception) {
            StringBuilder stb = new StringBuilder();

            if(exception != null) {
                stb.AppendLine(exception.GetType().Name).AppendLine(exception.Message).AppendLine(exception.StackTrace);
                stb.Append("Inner exception: ");
                if(exception.InnerException == null)
                    stb.Append("None");
                else {
                    stb.Append(FormatException(exception.InnerException));
                }
            }

            return stb.ToString();
        }
        protected virtual void InvokeMethod(string methodName, params string[] parameters) {
            StringBuilder sb = new StringBuilder();
            sb.Append("window.external.").Append(methodName).Append("(");
            for(int i = 0; i < parameters.Length; i++) {
                sb.Append("'" + EscapeString(parameters[i]) + "'");
                if(i < parameters.Length - 1) sb.Append(", ");
            }
            sb.Append(");");
            try {
                OutputString(sb.ToString());
            } catch {
            }
        }
        void ProcessResultMessage(LogMessage logMessage) {
            totalCount++;
            string testClassName = ((BaseTestClass)logMessage.Decorators[UnitTestLogDecorator.TestClassMetadata]).Type.ToString();
            string testMethodName = (string)logMessage.Decorators[LogDecorator.NameProperty];
            switch((TestOutcome)logMessage.Decorators[LogDecorator.TestOutcome]) {
                case TestOutcome.Passed:
                    InvokeMethod("TestResult", testClassName, testMethodName, "");
                    passedCount++;
                    break;
                case TestOutcome.Failed:
                    string exceptionString = FormatException(((ScenarioResult)logMessage.Decorators[UnitTestLogDecorator.ScenarioResult]).Exception);
                    InvokeMethod("TestResult", testClassName, testMethodName, exceptionString);
                    break;
                default:
                    break;
            }
        }
        void ProcessStartMessage() {
            InvokeMethod("Start", "");
        }
    }

    class SilverlightTestGUILog : TestLogger {
        protected override void OutputString(string text) {
            HtmlPage.Window.Eval(text);
        }
    }

    class DebugLogger : TestLogger {
        protected override void OutputString(string text) {
            Debug.WriteLine(text);
        }
    }
}