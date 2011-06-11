// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace DumpTestResultSummary
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    class Program
    {
        static int Main(string[] args)
        {
            var oldColor = Console.ForegroundColor;

            try
            {
                var mode = AnalysisMode.Summary;
                if (args.Length < 2)
                {
                    Usage();
                    return 1;
                }

                string trxFileName = args[1];
                if (!File.Exists(trxFileName))
                {
                    Console.Write(args[0].PadRight(35));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("TRX file not found!");
                    return 0;
                }

                if (args.Length >= 3)
                {
                    if (!Enum.TryParse(args[2], true, out mode))
                    {
                        Usage();
                        return 1;
                    }
                }

                XElement element = XElement.Load(trxFileName);
                var analyzer = new MSTestResultsFileAnalyzer(element.Name.Namespace);
                analyzer.Label = args[0];
                analyzer.Mode = mode;
                analyzer.DumpSummary(element);

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: DumpTestResultSummary label filename.trx [summary|detailed]");
        }
    }
}
