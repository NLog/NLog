Module Module1
    Dim logger As NLog.Logger = LogManager.GetLogger("Logger1")
    Sub Main()
        logger.Debug("Hello {0}", "world!")
    End Sub
End Module
