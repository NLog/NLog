using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace UpdateBuildNumber
{
    class Program
    {
        static int GetSubversionRevision(string workingDir)
        {
            string[] svnDirs = { ".svn", "_svn" };

            // try to determine the revision without spawning external EXE

            foreach (string wd in svnDirs)
            {
                string dir = Path.Combine(workingDir, wd);
                if (!Directory.Exists(dir))
                    continue;


                string f = Path.Combine(dir, "entries");

                // pre-1.4 XML format
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(f);

                    foreach (XmlElement el in doc.DocumentElement.ChildNodes)
                    {
                        if (el.GetAttribute("name") == String.Empty)
                        {
                            return Convert.ToInt32(el.GetAttribute("revision"));
                        }
                    }
                }
                catch
                {
                    // ignore errors
                }

                try
                {
                    // subversion 1.4 format
                    using (StreamReader sr = File.OpenText(f))
                    {
                        string version = sr.ReadLine().Trim();
                        if (version == "8")
                        {
                            if (sr.ReadLine().Trim() == "")
                            {
                                if (sr.ReadLine().Trim() == "dir")
                                {
                                    string rev = sr.ReadLine().Trim();
                                    return Convert.ToInt32(rev);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            try
            {
                using (Process p = new Process())
                {
                    // Console.WriteLine("Spawning 'svnversion' in {0}", workingDir);
                    p.StartInfo.WorkingDirectory = workingDir;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.Arguments = ".";
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "svnversion";
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        StringBuilder digits = new StringBuilder();
                        string line = p.StandardOutput.ReadLine();
                        foreach (char c in line)
                        {
                            if (!Char.IsDigit(c))
                                break;

                            digits.Append(c);
                        }

                        if (digits.Length > 0)
                            return Convert.ToInt32(digits.ToString());
                        else
                            return 0;
                    }
                    else
                    {
                        throw new Exception("Process 'svnversion' has exited with error exit code: " + p.ExitCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot determine working copy revision.", ex);
            }
        }

        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("USAGE: UpdateBuildNumber versionFile assemblyInfoFile actualVersionFile");
                Console.WriteLine();
                Console.WriteLine("Scans for [assembly: AssemblyVersion(*)] in the assemblyInfoFile");
                Console.WriteLine("and updates it to the version number found in versionFile.");
                Console.WriteLine("If the last component of the version number is zero, the tool");
                Console.WriteLine("replaces it with the Subversion revision number.");
                return 1;

            }

            try
            {
                string versionFile = args[0];
                string assemblyInfoFile = args[1];
                string actualVersionFile = args[2];
                string versionString;

                using (StreamReader sr = new StreamReader(versionFile))
                {
                    versionString = sr.ReadLine();
                }
                string[] versionParts = versionString.Split('.');
                if (versionParts.Length != 4)
                    throw new Exception("Invalid version number in " + versionFile);

                if (versionParts[3] == "*")
                {
                    versionParts[3] = GetSubversionRevision(Path.GetDirectoryName(Path.GetFullPath(versionFile))).ToString();
                }

                string newVersionString = String.Join(".", versionParts);

                bool modified = false;
                ArrayList lines = new ArrayList();
                if (File.Exists(assemblyInfoFile))
                {
                    using (StreamReader sr = new StreamReader(assemblyInfoFile, Encoding.Default))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.IndexOf("AssemblyVersion(") >= 0)
                            {
                                string newLine = "[assembly: AssemblyVersion(\"" + newVersionString + "\")]";
                                if (newLine != line)
                                {
                                    modified = true;
                                    line = newLine;
                                }
                            }
                            lines.Add(line);
                        }
                    }
                }
                else
                {
                    lines.Add("// do not modify this file. It will be automatically regenerated");
                    lines.Add("// based on the version number saved in '" + versionFile + "'");
                    lines.Add("using System.Reflection;");
                    lines.Add("[assembly: AssemblyVersion(\"" + newVersionString + "\")]");
                    modified = true;
                }

                if (modified)
                {
                    Console.WriteLine("Build number changed to '" + newVersionString + "'. Updating " + assemblyInfoFile + "'.");
                    using (StreamWriter sw = new StreamWriter(assemblyInfoFile, false, Encoding.Default))
                    {
                        foreach (string line in lines)
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Build number in '" + assemblyInfoFile + "' is up-to-date.");
                }
                using (StreamWriter sw = new StreamWriter(actualVersionFile, false, Encoding.Default))
                {
                    sw.WriteLine(newVersionString);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }
    }
}
