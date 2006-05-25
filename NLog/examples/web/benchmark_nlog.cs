Logger logger = LogManager.GetLogger("somelogger");

// Case 1. No formatting
logger.Debug("This is a message without formatting.");

// Case 2. One format parameter
logger.Debug("This is a message with {0} format parameter", 1);

// Case 3. Two format parameters
logger.Debug("This is a message with {0}{1} parameters", 2, "o");

// Case 4. Three format parameters
logger.Debug("This is a message with {0}{1}{2} parameters", "thr", 3, 3);

// Case 1a. No formatting, using a guard.
if (logger.IsDebugEnabled)
    logger.Debug("This is a message without formatting.");

// Case 2a. One format parameter, using a guard.
if (logger.IsDebugEnabled)
    logger.Debug("This is a message with {0} format parameter", 1);

// Case 3a. Two format parameters, using a guard.
if (logger.IsDebugEnabled)
    logger.Debug("This is a message with {0}{1} parameters", 2, "o");

// Case 4a. Three format parameters, using a guard.
if (logger.IsDebugEnabled)
    logger.Debug("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
