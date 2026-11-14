using System;
using System.Collections.Generic;

try
{
    var apiFiles = new List<string>();
    string outputFile = "NLog.xsd";
    string targetNamespace = "http://www.nlog-project.org/schemas/NLog.xsd";

    for (int i = 0; i < args.Length; ++i)
    {
        string argName;
        if (args[i].StartsWith("/") || args[i].StartsWith("-"))
        {
            argName = args[i].Substring(1).ToLowerInvariant();
        }
        else
        {
            argName = "?";
        }

        switch (argName)
        {
            case "api":
                apiFiles.Add(args[++i]);
                break;

            case "out":
                outputFile = args[++i];
                break;

            case "xmlns":
                targetNamespace = args[++i];
                break;

            default:
                Usage();
                return 1;
        }
    }

    if (apiFiles.Count == 0)
    {
        Console.WriteLine("API files not specified.");
        Usage();
        return 1;
    }

    if (string.IsNullOrEmpty(outputFile))
    {
        Console.WriteLine("Output file not specified.");
        Usage();
        return 1;
    }

    var xsdFileGenerator = new MakeNLogXSD.XsdFileGenerator
    {
        TargetNamespace = targetNamespace
    };

    foreach (string apiFile in apiFiles)
    {
        xsdFileGenerator.ProcessApiFile(apiFile);
    }

    xsdFileGenerator.SaveResult(outputFile);
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine("ERROR: {0}", ex);
    return 1;
}

static void Usage()
{
    Console.WriteLine("MakeNLogXSD -api inputFile.api [-out outputFile] [-xmlns namespace]");
}
