using NLog.SourceCodeTests;

var tests = new SourceCodeTests();
var fileHeadersValid = tests.VerifyFileHeaders();
var namespacesAndClassNamesValid = tests.VerifyNamespacesAndClassNames();
var success = fileHeadersValid && namespacesAndClassNamesValid;

if (success)
{
    Console.WriteLine("All source code checks passed.");
}

return success ? 0 : 1;
