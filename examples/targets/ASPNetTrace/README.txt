This is an example of ASP.NET trace target.

To test it:

1. Create a virtual directory in IIS:

    Virtual Directory alias: ASPNetTraceTest
    Local path:              <full path to this directory>
    Permissions:             Read & Run Scripts

2. Open the ASPNetTraceTest.sln solution in VS.NET 2003

3. Fix a reference to "NLog.dll" in this project (VS.NET is not very
good at locating external assemblies). Use NLog.dll that comes with 
your binary release of NLog or compile it yourself. You may also want 
to add extra NLog.dlls (NLog.Win32.dll, NLog.DotNet.dll) depending on 
your target needs.

4. Right-click on "test.aspx" and choose "Set As Start Page"

5. Compile and run (Ctrl-F5). This should spawn Internet Explorer, navigate
to http://localhost/ASPNetTraceTest/test.aspx and display "loaded!".

6. Now, navigate to http://localhost/ASPNetTraceTest/Trace.axd and you 
should see your logs.

7. You may customize your log settings through Web.nlog which is a 
recommended way of configuring ASP.NET applications.