using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NLog.SourceCodeTests
{
    /// <summary>
    /// Source code tests.
    /// </summary>
    public class SourceCodeTests
    {
        private static readonly Regex ClassNameRegex = new Regex(@"^\s+(public |abstract |sealed |static |partial |internal )*\s*(class|interface|struct|enum)\s+(?<className>\w+)\b", RegexOptions.Compiled);
        private static readonly Regex DelegateTypeRegex = new Regex(@"^    (public |internal )delegate .*\b(?<delegateType>\w+)\(", RegexOptions.Compiled);
        private static List<string> _directoriesToVerify;
        private readonly bool _verifyNamespaces;

        private readonly IList<string> _fileNamesToIgnore;

        private readonly string _rootDir;
        private string _licenseFile;
        private readonly string[] _licenseLines;

        public SourceCodeTests()
        {
            _rootDir = FindRootDir();
            _directoriesToVerify = GetAppSettingAsList("VerifyFiles.Paths");
            _fileNamesToIgnore = GetAppSettingAsList("VerifyFiles.IgnoreFiles");
            _verifyNamespaces = false; //off for now (april 2019) - so we could create folders and don't break stuff
            if (_rootDir != null)
            {
                _licenseLines = File.ReadAllLines(_licenseFile);
            }
            else
            {
                throw new Exception("root not found (where LICENSE.txt is located)");
            }
        }

        private static List<string> GetAppSettingAsList(string setting)
        {
            return ConfigurationManager.AppSettings[setting].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        /// Find source root by finding LICENSE.txt
        /// </summary>
        private string FindRootDir()
        {
            var dir = ConfigurationManager.AppSettings["rootdir"];
            dir = Path.GetFullPath(dir);
            while (dir != null)
            {
                _licenseFile = Path.Combine(dir, "LICENSE.txt");
                if (File.Exists(_licenseFile))
                {
                    break;
                }

                dir = Path.GetDirectoryName(dir);
            }
            return dir;
        }


        public bool VerifyFileHeaders()
        {
            var missing = FindFilesWithMissingHeaders().ToList();
            return ReportErrors(missing, "Missing headers (copy them form other another file).");
        }

        private IEnumerable<string> FindFilesWithMissingHeaders()
        {
            foreach (string dir in _directoriesToVerify)
            {
                foreach (string file in Directory.GetFiles(Path.Combine(_rootDir, dir), "*.cs", SearchOption.AllDirectories))
                {
                    if (ShouldIgnoreFileForVerify(file))
                    {
                        continue;
                    }

                    if (!VerifyFileHeader(file))
                    {
                        yield return file;
                    }
                }
            }
        }

        public bool VerifyNamespacesAndClassNames()
        {
            var errors = new List<string>();

            foreach (string dir in _directoriesToVerify)
            {
                VerifyClassNames(Path.Combine(_rootDir, dir), Path.GetFileName(dir), errors);
            }

            return ReportErrors(errors, "Namespace or classname not in-sync with file name.");
        }

        bool ShouldIgnoreFileForVerify(string filePath)
        {
            string baseName = Path.GetFileName(filePath);
            if (baseName == null || _fileNamesToIgnore.Contains(baseName))
            {
                return true;
            }

            if (baseName.IndexOf(".xaml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (baseName.IndexOf(".g.", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (baseName.IndexOf(".designer.", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (baseName == "ExtensionAttribute.cs")
            {
                return true;
            }

            if (baseName == "NUnitAdapter.cs")
            {
                return true;
            }

            if (baseName == "LocalizableAttribute.cs")
            {
                return true;
            }

            if (baseName == "Annotations.cs")
            {
                return true;
            }

            return false;
        }

        private bool VerifyFileHeader(string filePath)
        {

            if (FileInObjFolder(filePath))
            {
                //don't scan files in obj folder
                return true;
            }

            using (StreamReader reader = File.OpenText(filePath))
            {
                for (int i = 0; i < _licenseLines.Length; ++i)
                {
                    string line = reader.ReadLine();
                    string expected = "// " + _licenseLines[i];
                    if (line != expected)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static bool FileInObjFolder(string path)
        {
            return path.Contains("/obj/") || path.Contains("\\obj\\")
                   || path.StartsWith("obj/", StringComparison.InvariantCultureIgnoreCase) || path.StartsWith("obj\\", StringComparison.InvariantCultureIgnoreCase);
        }

        private void VerifyClassNames(string path, string expectedNamespace, List<string> errors)
        {

            if (FileInObjFolder(path))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(path, "*.cs"))
            {
                if (ShouldIgnoreFileForVerify(filePath))
                {
                    continue;
                }

                string expectedClassName = Path.GetFileNameWithoutExtension(filePath);

                if (expectedClassName != null)
                {
                    int p = expectedClassName.IndexOf('-');
                    if (p >= 0)
                    {
                        expectedClassName = expectedClassName.Substring(0, p);
                    }

                    var fileErrors = VerifySingleFile(filePath, expectedNamespace, expectedClassName);
                    errors.AddRange(fileErrors.Select(errorMessage => $"{filePath}:{errorMessage}"));
                }

            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                VerifyClassNames(dir, expectedNamespace + "." + Path.GetFileName(dir), errors);
            }

        }

        ///<summary>Verify classname and namespace in a file.</summary>
        ///<returns>errors</returns>
        private IEnumerable<string> VerifySingleFile(string filePath, string expectedNamespace, string expectedClassName)
        {
            //ignore list
            if (filePath != null && !filePath.EndsWith("nunit.cs", StringComparison.InvariantCultureIgnoreCase))
            {
                HashSet<string> classNames = new HashSet<string>();
                using (StreamReader sr = File.OpenText(filePath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (_verifyNamespaces)
                        {
                            if (line.StartsWith("namespace ", StringComparison.Ordinal))
                            {
                                string ns = line.Substring(10);
                                if (expectedNamespace != ns)
                                {
                                    yield return $"Invalid namespace: '{ns}' Expected: '{expectedNamespace}'";
                                }
                            }
                        }

                        Match match = ClassNameRegex.Match(line);
                        if (match.Success)
                        {
                            classNames.Add(match.Groups["className"].Value);
                        }

                        match = DelegateTypeRegex.Match(line);
                        if (match.Success)
                        {
                            classNames.Add(match.Groups["delegateType"].Value);
                        }
                    }
                }

                if (classNames.Count == 0)
                {
                    //Console.WriteLine("No classes found in {0}", file);

                    //ignore, because of files not used in other projects
                }
                else if (!classNames.Contains(expectedClassName))
                {
                    yield return $"Invalid class name. Expected '{expectedClassName}', actual: '{string.Join(",", classNames)}'";
                }
            }
        }

        private static bool ReportErrors(List<string> errors, string globalErrorMessage)
        {
            var count = errors.Count;
            if (count == 0)
            {
                return true;
            }

            var fullMessage = $"{globalErrorMessage}\n{count} errors: \n -------- \n-{string.Join("\n- ", errors)} \n\n";
            Console.Error.WriteLine(fullMessage);
            return false;
        }
    }
}