using System.Text.RegularExpressions;

namespace NLog.SourceCodeTests;

public sealed class SourceCodeTests
{
    private static readonly Regex ClassNameRegex = new(
        @"^\s+(public |abstract |sealed |static |partial |internal )*\s*(class|interface|struct|enum)\s+(?<className>\w+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DelegateTypeRegex = new(
        @"^    (public |internal )delegate .*\b(?<delegateType>\w+)\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] DirectoriesToVerify = ["src/NLog", "tests/NLog.UnitTests"];
    private static readonly HashSet<string> FileNamesToIgnore = new(StringComparer.OrdinalIgnoreCase)
    {
        "AssemblyInfo.cs",
        "AssemblyBuildInfo.cs",
        "GlobalSuppressions.cs",
        "CompilerAttributes.cs",
        "Logger-generated.cs",
        "ExtensionAttribute.cs",
        "NUnitAdapter.cs",
        "LocalizableAttribute.cs",
        "Annotations.cs"
    };

    private readonly string _rootDirectory;
    private readonly string[] _licenseLines;

    public SourceCodeTests()
    {
        _rootDirectory = FindRootDirectory(AppContext.BaseDirectory);
        _licenseLines = File.ReadAllLines(Path.Combine(_rootDirectory, "LICENSE.txt"));
    }

    public bool VerifyFileHeaders()
    {
        var missing = DirectoriesToVerify
            .SelectMany(directory => Directory.EnumerateFiles(
                Path.Combine(_rootDirectory, directory), "*.cs", SearchOption.AllDirectories))
            .Where(file => !ShouldIgnoreFileForVerify(file))
            .Where(file => !VerifyFileHeader(file))
            .ToList();

        return ReportErrors(missing, "Missing source file headers.");
    }

    public bool VerifyNamespacesAndClassNames()
    {
        var errors = new List<string>();
        foreach (var directory in DirectoriesToVerify)
        {
            VerifyClassNames(Path.Combine(_rootDirectory, directory), errors);
        }

        return ReportErrors(errors, "Class name does not match the file name.");
    }

    private static string FindRootDirectory(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LICENSE.txt")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find the repository root containing LICENSE.txt from '{startDirectory}'.");
    }

    private static bool ShouldIgnoreFileForVerify(string filePath)
    {
        var baseName = Path.GetFileName(filePath);
        return FileNamesToIgnore.Contains(baseName)
            || baseName.Contains(".xaml", StringComparison.OrdinalIgnoreCase)
            || baseName.Contains(".g.", StringComparison.OrdinalIgnoreCase)
            || baseName.Contains(".designer.", StringComparison.OrdinalIgnoreCase);
    }

    private bool VerifyFileHeader(string filePath)
    {
        if (FileInObjFolder(filePath))
        {
            return true;
        }

        using var lines = File.ReadLines(filePath).GetEnumerator();
        foreach (var licenseLine in _licenseLines)
        {
            if (!lines.MoveNext() || lines.Current != ("// " + licenseLine).TrimEnd())
            {
                return false;
            }
        }

        return true;
    }

    private static bool FileInObjFolder(string path)
    {
        return path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private void VerifyClassNames(string path, List<string> errors)
    {
        if (FileInObjFolder(path))
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(path, "*.cs"))
        {
            if (ShouldIgnoreFileForVerify(filePath))
            {
                continue;
            }

            var expectedClassName = Path.GetFileNameWithoutExtension(filePath);
            var suffixIndex = expectedClassName.IndexOf('-');
            if (suffixIndex >= 0)
            {
                expectedClassName = expectedClassName[..suffixIndex];
            }

            var fileErrors = VerifySingleFile(filePath, expectedClassName);
            errors.AddRange(fileErrors.Select(errorMessage => $"{filePath}:{errorMessage}"));
        }

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            VerifyClassNames(directory, errors);
        }
    }

    private static IEnumerable<string> VerifySingleFile(string filePath, string expectedClassName)
    {
        if (filePath.EndsWith("nunit.cs", StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var classNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(filePath))
        {
            var match = ClassNameRegex.Match(line);
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

        if (classNames.Count > 0 && !classNames.Contains(expectedClassName))
        {
            yield return $"Invalid class name. Expected '{expectedClassName}', actual: '{string.Join(",", classNames)}'";
        }
    }

    private static bool ReportErrors(List<string> errors, string globalErrorMessage)
    {
        if (errors.Count == 0)
        {
            return true;
        }

        Console.Error.WriteLine(
            $"{globalErrorMessage}{Environment.NewLine}{errors.Count} errors:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        return false;
    }
}
