using NLog;

class MyClass {
    // storing logger reference in a static variable is clean and fast
    static Logger logger = LogManager.GetLogger("MyClass");

    static void Main()
    {
        logger.Debug("This is a debugging message");

        // it is not recommended to get the logger and store it in a local variable
        Logger logger2 = LogManager.GetLogger("MyClass");
        logger2.Debug("This is a debugging message");
    }
}

