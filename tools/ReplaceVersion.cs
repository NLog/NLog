// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using System;
using System.IO;

public class ReplaceVersion
{
    static string fileNamePattern = "AssemblyInfo.cs";
    static string newAssemblyVersion;

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ReplaceVersion.exe <root_dir> <new_version_number>");
            Console.WriteLine();
            Console.WriteLine("Scans for AssemblyInfo.cs file in <root_dir> and subdirectories");
            Console.WriteLine("and replaces [assembly: AssemblyVersion()] with given version number");
            return;
        }
        string dirName = args[0];
        newAssemblyVersion = args[1];

        DirectoryInfo rootDir = new DirectoryInfo(dirName);
        ScanDirectory(rootDir);
    }

    private static void ScanDirectory(DirectoryInfo di)
    {
        string fullName = Path.Combine(di.FullName, fileNamePattern);

        if (File.Exists(fullName))
        {
            UpdateFile(fullName, di);
        }
        foreach (DirectoryInfo subdir in di.GetDirectories())
        {
            ScanDirectory(subdir);
        }
    }

    private static void UpdateFile(string fullName, DirectoryInfo di)
    {
        string tmp = fullName + ".tmp";
        try
        {
            Console.Write("{0}: ", di.FullName);
            bool changed = false;
            using (StreamWriter sw = new StreamWriter(tmp))
            {
                using (StreamReader sr = new StreamReader(fullName))
                {
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("[assembly: AssemblyVersion("))
                        {
                            string newValue = String.Format("[assembly: AssemblyVersion(\"{0}\")]", newAssemblyVersion);
                            sw.WriteLine(newValue);
                            if (line != newValue)
                            {
                                changed = true;
                            }
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            if (changed)
            {
                Console.WriteLine("Done.");
                File.Delete(fullName);
                File.Move(tmp, fullName);
            }
            else
            {
                Console.WriteLine("No change.");
                File.Delete(tmp);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e.Message);
            File.Delete(tmp);
        }
    }
}
