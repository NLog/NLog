namespace DumpApiXml
{
    using System;
    using System.IO;

    public class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var builder = new DocFileBuilder();
                string outputFile = null;
                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-comments":
                            builder.LoadComments(args[++i]);
                            break;

                        case "-assembly":
                            {
                                string assembly = args[++i];
                                if (File.Exists(assembly))
                                {
                                    builder.LoadAssembly(assembly);
                                    string docpath = Path.ChangeExtension(assembly, ".xml");
                                    if (File.Exists(docpath))
                                    {
                                        builder.LoadComments(docpath);
                                    }
                                }
                            }
                            break;

                        case "-nc":
                            builder.DisableComments();
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

                outputFile = Path.GetFullPath(outputFile);
                Console.WriteLine("Generating '{0}'...", outputFile);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
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
            Console.WriteLine("DumpApiXml [-comments commentFile.xml]+ [-assembly assembly.dll] -output file.xml");
        }
    }
}