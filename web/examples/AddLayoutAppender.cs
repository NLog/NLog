static void Main(string[] args)
{
    LayoutAppenderFactory.AddLayoutAppender("hour", typeof(MyNamespace.MyFirstLayoutAppender));

    // start logging here
}
