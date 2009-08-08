using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;

namespace MakeNLogDoc
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                DocFileBuilder builder = new DocFileBuilder();
                string outputFile = null;
                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-comments":
                            builder.LoadComments(args[++i]);
                            break;

                        case "-ref":
                            builder.AddReferenceDirectory(args[++i]);
                            break;

                        case "-assembly":
                            builder.LoadAssembly(args[++i]);
                            break;

                        case "-output":
                            outputFile = args[++i];
                            break;

                        case "-?":
                            Usage();
                            return 0;

                        default:
                            Console.WriteLine("Unknown option '{0}'", args[i]);
                            Usage();
                            return 1;
                    }
                }
                if (outputFile == null)
                {
                    Usage();
                    return 1;
                }

                builder.Build(outputFile);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.ToString());
                return 1;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("MakeNLogXSD [-comments commentFile.xml]+ [-ref referenceDirectory ] [-assembly assembly.dll] -output file.xml");
        }
    }
}