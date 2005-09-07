#if LOG4NET_WITH_FASTLOGGER
using System;
using System.Globalization;
using log4net.Core;

public sealed class FastLogger : LoggerWrapperImpl
{
    private readonly static Type declaringType = typeof(FastLogger);
    public FastLogger(ILogger logger) : base(logger)
    {

    }

    public bool IsDebugEnabled
    {
        get { return Logger.IsEnabledFor(Level.Debug); }
    }
    
    public void Debug(string message)
    {
        Logger.Log(declaringType, Level.Debug, message, null);
    }
    public void DebugFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Debug))
        {
            Logger.Log(declaringType, Level.Debug, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
    
    public bool IsInfoEnabled
    {
        get { return Logger.IsEnabledFor(Level.Info); }
    }
    
    public void Info(string message)
    {
        Logger.Log(declaringType, Level.Info, message, null);
    }
    public void InfoFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Info))
        {
            Logger.Log(declaringType, Level.Info, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
}

#endif
