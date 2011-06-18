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

namespace InstallNLogConfig
{
    using System;
    using System.IO;
    using NLog.Config;
    using NLog;

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using (var context = new InstallationContext())
                {
                    context.LogOutput = Console.Out;

                    XmlLoggingConfiguration configuration = null;

                    bool uninstallMode = false;

                    for (int i = 0; i < args.Length; ++i)
                    {
                        switch (args[i])
                        {
                            case "-q":
                                context.LogOutput = TextWriter.Null;
                                break;

                            case "-consolelog":
                                context.LogOutput = Console.Out;
                                break;

                            case "-loglevel":
                                context.LogLevel = LogLevel.FromString(args[++i]);
                                break;

                            case "-i":
                                context.IgnoreFailures = true;
                                break;

                            case "-log":
                                context.LogOutput = File.CreateText(args[++i]);
                                break;

                            case "-p":
                                string arg = args[++i];
                                int p = arg.IndexOf('=');
                                if (p < 0)
                                {
                                    Console.WriteLine("Parameter '{0}' must be NAME=VALUE", arg);
                                    Usage();
                                    return 1;
                                }

                                string paramName = arg.Substring(0, p);
                                string paramValue = arg.Substring(p + 1);
                                context.Parameters.Add(paramName, paramValue);
                                break;

                            case "-u":
                                uninstallMode = true;
                                break;

                            case "-?":
                                Usage();
                                return 0;

                            default:
                                if (args[i].StartsWith("-"))
                                {
                                    Usage();
                                    return 1;
                                }

                                configuration = new XmlLoggingConfiguration(args[i]);
                                break;
                        }
                    }

                    if (configuration == null)
                    {
                        Usage();
                        return 1;
                    }

                    if (uninstallMode)
                    {
                        configuration.Uninstall(context);
                    }
                    else
                    {
                        configuration.Install(context);
                    }

                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: {0}", ex);
                return 1;
            }
        }

        /// <summary>
        /// Displays the usage.
        /// </summary>
        static void Usage()
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage: InstallNLogConfig [options] NLog.config...");
            Console.ForegroundColor = oldColor;
            Console.WriteLine();
            Console.WriteLine("Performs installation/uninstallation that requires administrative permissions");
            Console.WriteLine("(such as Event Log sources, databases, etc).");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -u                uninstall");
            Console.WriteLine("  -log file.txt     save installation log to a file");
            Console.WriteLine("  -q                quiet (do not write a log)");
            Console.WriteLine("  -i                ignore failures");
            Console.WriteLine("  -consolelog       write installation log to the console");
            Console.WriteLine("  -loglevel level   set log level (Trace, Debug, Info, Warn, Error or Fatal)");
            Console.WriteLine("  -p NAME=VALUE     set installation parameter value");
            Console.WriteLine();
            Console.WriteLine("Parameters can be referenced in NLog.config using ${install-context:NAME}");
        }
    }
}
