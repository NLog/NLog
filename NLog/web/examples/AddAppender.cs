static void Main(string[] args)
{
    AppenderFactory.AddAppender("MyFirst", typeof(MyNamespace.MyFirstAppender));

    // start logging here
}
