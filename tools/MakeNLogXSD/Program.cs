// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace MakeNLogXSD
{
    using System;
    using System.Collections.Generic;

    class Program
    {
        public static int Main(string[] args)
        {
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

                var xsdFileGenerator = new XsdFileGenerator();
                xsdFileGenerator.TargetNamespace = targetNamespace;

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
        }

        private static void Usage()
        {
            Console.WriteLine("MakeNLogXSD -api inputFile.api [-out outputFile] [-xmlns namespace]");
        }
    }
}
