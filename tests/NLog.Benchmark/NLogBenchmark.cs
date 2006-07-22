using NLog;

namespace NLog.Benchmark
{
    class NLogBenchmark : IBenchmark
    {
        public string Header
        {
            get { 
                return @"using NLog;
using NLog.Config;";
            }
        }

        public string Footer
        {
            get
            {
                return "";
            }
        }

        public string CreateSource(string variableName, string name)
        {
            return "static Logger " + variableName + " = LogManager.GetLogger(\"" + name + "\");";
        }

        public string WriteUnformatted(string loggerVariable, string level, string text)
        {
            return loggerVariable + "." + level + "(\"" + text + "\");";
        }

        public string WriteFormatted(string loggerVariable, string level, string text, string par)
        {
            return loggerVariable + "." + level + "(\"" + text + "\", " + par + ");";
        }

        public string GuardedWrite(string loggerVariable, string level, string text, string par)
        {
            return "if (" + loggerVariable + ".Is" + level + "Enabled) " + loggerVariable + "." + level + "(\"" + text + "\", " + par + ");";
        }

        public string[] References
        {
            get { return new string[] { "NLog.dll" }; }
        }

        public string Name
        {
            get { return "NLog"; }
        }

        public string Init
        {
            get { return "LogManager.Configuration = new XmlLoggingConfiguration(\"NLog.config\");"; }
        }

        public string Flush
        {
            get { return "LogManager.Flush();"; }
        }
    }
}