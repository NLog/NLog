using NLog;

namespace NLog.Benchmark
{
    class Log4NetBenchmark : IBenchmark
    {
        protected string Log4NetCommonHeader
        {
            get { 
                return @"using log4net;

using log4net.Config;
using log4net.Core;
using log4net.Appender;
using log4net.Util;
using System.Threading;

public class NullAppender : AppenderSkeleton
{
    override protected void Append(LoggingEvent loggingEvent) 
    {
    }
}

public class NullAppenderWithLayout : AppenderSkeleton
{
    override protected void Append(LoggingEvent loggingEvent) 
    {
        string s = RenderLoggingEvent(loggingEvent);
        // ignore s
    }
}

public sealed class AsyncAppender : IAppender, IBulkAppender, IOptionHandler, IAppenderAttachable
{
	private string m_name;

	public string Name
	{
		get { return m_name; }
		set { m_name = value; }
	}

	public void ActivateOptions() 
	{
	}

	public FixFlags Fix
	{
		get { return m_fixFlags; }
		set { m_fixFlags = value; }
	}

	public void Close()
	{
		// Remove all the attached appenders
		lock(this)
		{
			if (m_appenderAttachedImpl != null)
			{
				m_appenderAttachedImpl.RemoveAllAppenders();
			}
		}
	}

	public void DoAppend(LoggingEvent loggingEvent)
	{
		loggingEvent.Fix = m_fixFlags;
		System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncAppend), loggingEvent);
	}

	public void DoAppend(LoggingEvent[] loggingEvents)
	{
		foreach(LoggingEvent loggingEvent in loggingEvents)
		{
			loggingEvent.Fix = m_fixFlags;
		}
		System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncAppend), loggingEvents);
	}

	private void AsyncAppend(object state)
	{
		if (m_appenderAttachedImpl != null)
		{
			LoggingEvent loggingEvent = state as LoggingEvent;
			if (loggingEvent != null)
			{
				m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
			}
			else
			{
				LoggingEvent[] loggingEvents = state as LoggingEvent[];
				if (loggingEvents != null)
				{
					m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvents);
				}
			}
		}
	}

	public void AddAppender(IAppender newAppender) 
	{
		if (newAppender == null)
		{
			throw new ArgumentNullException(""newAppender"");
		}
		lock(this)
		{
			if (m_appenderAttachedImpl == null) 
			{
				m_appenderAttachedImpl = new log4net.Util.AppenderAttachedImpl();
			}
			m_appenderAttachedImpl.AddAppender(newAppender);
		}
	}

	public AppenderCollection Appenders
	{
		get
		{
			lock(this)
			{
				if (m_appenderAttachedImpl == null)
				{
					return AppenderCollection.EmptyCollection;
				}
				else 
				{
					return m_appenderAttachedImpl.Appenders;
				}
			}
		}
	}

	public IAppender GetAppender(string name) 
	{
		lock(this)
		{
			if (m_appenderAttachedImpl == null || name == null)
			{
				return null;
			}

			return m_appenderAttachedImpl.GetAppender(name);
		}
	}

	public void RemoveAllAppenders() 
	{
		lock(this)
		{
			if (m_appenderAttachedImpl != null) 
			{
				m_appenderAttachedImpl.RemoveAllAppenders();
				m_appenderAttachedImpl = null;
			}
		}
	}

	public IAppender RemoveAppender(IAppender appender) 
	{
		lock(this)
		{
			if (appender != null && m_appenderAttachedImpl != null) 
			{
				return m_appenderAttachedImpl.RemoveAppender(appender);
			}
		}
		return null;
	}

	public IAppender RemoveAppender(string name) 
	{
		lock(this)
		{
			if (name != null && m_appenderAttachedImpl != null)
			{
				return m_appenderAttachedImpl.RemoveAppender(name);
			}
		}
		return null;
	}

	private AppenderAttachedImpl m_appenderAttachedImpl;
	private FixFlags m_fixFlags = FixFlags.All;
}
";
            }
        }

        public virtual string Header
        {
            get { 
                return Log4NetCommonHeader + @"
[assembly: XmlConfigurator(ConfigFile=""log4net.config"")]";
            }
        }

        public string Footer
        {
            get
            {
                return "";
            }
        }

        public virtual string CreateSource(string variableName, string name)
        {
            return "static ILog " + variableName + " = LogManager.GetLogger(\"" + name + "\");";
        }

        public string WriteUnformatted(string loggerVariable, string level, string text)
        {
            return loggerVariable + "." + level + "(\"" + text + "\");";
        }

        public string WriteFormatted(string loggerVariable, string level, string text, string par)
        {
            return loggerVariable + "." + level + "Format(\"" + text + "\", " + par + ");";
        }

        public string GuardedWrite(string loggerVariable, string level, string text, string par)
        {
            return "if (" + loggerVariable + ".Is" + level + "Enabled) " + loggerVariable + "." + level + "Format(\"" + text + "\", " + par + ");";
        }

        public string[] References
        {
            get { return new string[] { "log4net.dll", "System.dll", "System.Xml.dll" }; }
        }
 
        public virtual string Name
        {
            get { return "Log4Net"; }
        }

        public virtual string Init
        {
            get { return "log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(\"Log4Net.config\"));"; }
        }

        public string Flush
        {
            get { return "LogManager.Shutdown();"; }
        }
   }            
}
