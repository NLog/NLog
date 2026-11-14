using NLog.SourceCodeTests;

var tests = new SourceCodeTests();
var success = tests.VerifyFileHeaders() & tests.VerifyNamespacesAndClassNames();

if (success)
{
    Console.WriteLine("All source code checks passed.");
}

return success ? 0 : 1;
