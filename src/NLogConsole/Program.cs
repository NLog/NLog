using NLog;
using NLog.Fluent;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, NLogConsole!");

Logger log = LogManager.GetCurrentClassLogger();
log.Trace("This is a Trace message FC");
//log.Debug("This is a debug message");
//log.Error(new Exception(), "This is an error message");
//log.Fatal("This is a fatal message");

//log.Info()
//    .Message("This is a Fluent Info Message")
//    .Property("TestProp", "SomeProp")
//    .Write();



Console.WriteLine("The End!");
Console.ReadLine();