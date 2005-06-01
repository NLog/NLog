#if LOG4NET
    ILog logger = LogManager.GetLogger("somelogger");
#else
    Logger logger = LogManager.GetLogger("somelogger");
#endif

// Case 1. No formatting:
logger.Debug("This is a message without formatting.");

// Case 2. One format parameter
#if LOG4NET
    logger.Debug(String.Format("This is a message with {0} format parameter", 1));
#else
    logger.Debug("This is a message with {0} format parameter", 1);
#endif

// Case 3. Two format parameters
#if LOG4NET
    logger.Debug(String.Format("This is a message with {0}{1} parameters", 2, "o"));
#else
    logger.Debug("This is a message with {0}{1} parameters", 2, "o");
#endif
    
// Case 4. Three format parameters
#if LOG4NET
    logger.Debug(String.Format("This is a message with {0}{1}{2} parameters", "thr", 3, 3));
#else
    logger.Debug("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
#endif

