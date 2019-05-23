using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
        private static readonly XNamespace MsBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly bool _verifyNamespaces;

        private readonly IList<string> _fileNamesToIgnore;

        private readonly List<string> _projectFolders;
        private readonly string _rootDir;
        private string _licenseFile;
        private readonly string[] _licenseLines;

        public SourceCodeTests()
        {
            _rootDir = FindRootDir();
            _directoriesToVerify = GetAppSettingAsList("VerifyFiles.Paths");
            _fileNamesToIgnore = GetAppSettingAsList("VerifyFiles.IgnoreFiles");
            _projectFolders = GetAppSettingAsList("projectFolders");
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

     


        /// <summary>assemblyToCheckPath
        /// Verify that all projects has the needed files.
        /// </summary>
        public bool VerifyProjectsInSync()
        {
            var success = true;
            foreach (var folder in _projectFolders)
            {
                success = success & CheckProjects(folder);
            }
            return success;
        }

        private bool CheckProjects(string projectDir)
        {
            var success = true;

            var root = Path.Combine(_rootDir, projectDir);
            List<string> filesToCompile = new List<string>(1024);
            GetAllFilesToCompileInDirectory(filesToCompile, root, "*.cs", "");

            var projects = new DirectoryInfo(root).GetFiles("*.csproj", SearchOption.AllDirectories);
            foreach (var project in projects)
            {
                success = success & CompareDirectoryWithProjects(filesToCompile, project.FullName, project.Name);
            }
            return success;
        }

        private static bool CompareDirectoryWithProjects(List<string> filesToCompile, string projectFullPath, string projectFileName)
        {
            var filesInProject = new List<string>();
            GetCompileItemsFromProjects(filesInProject, projectFullPath);

            var missingFiles = filesToCompile.Except(filesInProject).ToList();
           return ReportErrors(missingFiles, $"project '{projectFileName}' is missing files.\nRun 'msbuild NLog.proj /t:SyncProjectItems'.");

        }

        private static void GetCompileItemsFromProjects(List<string> filesInProject, string projectFullPath)
        {
            GetCompileItemsFromProject(filesInProject, projectFullPath);
        }

        private static void GetCompileItemsFromProject(List<string> filesInProject, string csproj)
        {
            XElement contents = XElement.Load(csproj);
            filesInProject.AddRange(contents.Descendants(MsBuildNamespace + "Compile").Select(c => (string)c.Attribute("Include")));
        }

        private static void GetAllFilesToCompileInDirectory(List<string> output, string path, string pattern, string prefix)
        {
            foreach (string file in Directory.GetFiles(path, pattern))
            {
                if (file.EndsWith(".xaml.cs"))
                {
                    continue;
                }

                if (file.Contains(".g."))
                {
                    continue;
                }
                if (file.IndexOf(".designer.", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (FileInObjFolder(prefix + file))
                {
                    continue;
                }

                output.Add(prefix + Path.GetFileName(file));
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                GetAllFilesToCompileInDirectory(output, dir, pattern, prefix + Path.GetFileName(dir) + "\\");
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

        /// <summary>
        /// Vertify that all properties with the <see cref="DefaultValueAttribute"/> are set with the default ctor.
        /// </summary>
        public bool VerifyDefaultValues()
        {
            var assemblyToCheck = ConfigurationManager.AppSettings["assemblyToCheckPath"];
            var assemblyToCheck2 = Path.GetFullPath(Path.Combine(_rootDir, assemblyToCheck));
            if (!File.Exists(assemblyToCheck2))
            {
                throw new FileNotFoundException(string.Format("Failed loading DLL from path: {0}", assemblyToCheck2));
            }

            var ass = Assembly.LoadFile(assemblyToCheck2);
            //var types = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(s => s.GetTypes());
            var types = ass.GetTypes();

            //  VerifyDefaultValuesType(typeof(MailTarget));
            List<string> errors = new List<string>();

            foreach (var type in types)
            {
                VerifyDefaultValuesType(type, errors);
            }

            //one message for all failing properties
            return ReportErrors(errors, "DefaultValueAttribute not in sync with initial value.");
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

        ///<summary>Verify all properties with the <see cref="DefaultValueAttribute"/></summary>
        ///<remarks>Note: Xunit dont like overloads</remarks>
        private static void VerifyDefaultValuesType(Type type, List<string> errors)
        {
            var props = type.GetProperties();

            var defaultValuesDict = new Dictionary<string, object>();

            //find first [DefaultValue] values of all props
            foreach (var propertyInfo in props)
            {

                var defaultValues = propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                if (defaultValues.Any())
                {
                    var firstDefaultValueAttr = (DefaultValueAttribute)defaultValues.First();

                    defaultValuesDict.Add(propertyInfo.Name, firstDefaultValueAttr.Value);
                }
            }
            if (defaultValuesDict.Any())
            {
                //find first ctor without parameters
                var ctor = type.GetConstructors().FirstOrDefault(c => !c.GetParameters().Any());
                if (ctor != null)
                {
                    var newObject = ctor.Invoke(null);

                    //check al needed props
                    foreach (var propertyInfo in props.Where(p => defaultValuesDict.ContainsKey(p.Name)))
                    {
                        var neededVal = defaultValuesDict[propertyInfo.Name];
                        var currentVal = propertyInfo.GetValue(newObject, null);


                        var eq = AreDefaultValuesEqual(neededVal, currentVal, propertyInfo);
                        if (!eq)
                        {
                            //report
                            string message =
                                $"{type.FullName}.{propertyInfo.Name} . DefaultValueAttribute = {PrintValForMessage(neededVal)}, ctor = {PrintValForMessage(currentVal)}";
                            errors.Add(message);
                        }
                    }
                }

            }

        }

        /// <summary>
        /// Are the values equal?
        /// </summary>
        /// <param name="neededVal">the val from the <see cref="DefaultValueAttribute"/></param>
        /// <param name="currentVal">the val from the empty ctor</param>
        /// <param name="propertyInfo">the prop where the value came from</param>
        /// <returns>equals</returns>
        private static bool AreDefaultValuesEqual(object neededVal, object currentVal, PropertyInfo propertyInfo)
        {
            if (neededVal == null)
            {
                if (currentVal != null)
                {
                    return false;

                }
                //both null, OK, next
                return true;
            }
            if (currentVal == null)
            {
                //needed was null, so wrong
                return false;
            }
            //try as strings first



            var propType = propertyInfo.PropertyType;
            var neededString = neededVal.ToString();
            var currentString = currentVal.ToString();

            var eqstring = neededString.Equals(currentString);
            if (eqstring)
            {
                //ok, so next
                return true;
            }



            //handle UTF-8 properly
            if (propType == typeof(Encoding))
            {

                if (currentVal is UTF8Encoding && (neededString.Equals("utf-8", StringComparison.InvariantCultureIgnoreCase) || neededString.Equals("utf8", StringComparison.InvariantCultureIgnoreCase)))
                    return true;

            }



            //nulls or not string equals, fallback
            //Assert.Equal(neededVal, currentVal);
            return neededVal.Equals(currentVal);

        }

        /// <summary>
        /// print value quoted or as NULL
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string PrintValForMessage(object o)
        {
            if (o == null) return "NULL";
            return "'" + o + "'";
        }


    }
}