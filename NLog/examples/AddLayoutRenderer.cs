static void Main(string[] args)
{
    LayoutRendererFactory.AddLayoutRenderer("hour", typeof(MyNamespace.MyFirstLayoutRenderer));

    // start logging here
}
