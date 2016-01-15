Pull request management
===

Reviewing
---
When reviewing a pull request, check the following:

- Ensure the pull request has a good, descriptive name.
- **Important:** Check for binary backwardscompatiblity. NB: new optional parameters are not binary compatible.
- **Important:** set the Milestone.
- Add the applicable Labels.  
  Eg.
  - Part: `file-target`, `nlog-configuration` etc
  - Type: `bug`, `enhancement`, `feature`, `performance`. And  `enhancement` is a change without functional impact. Small features are also labeled as `feature`.
  - Tests: `needs unittests`, `has unittests`
  - Status: `waiting for review`, `almost ready`, `ready for merge`
- Set the Assignee. It must indicate who is currently holding the ball.   
  For example, if you intend to review, assign to yourself. If, after the review, some changes need to be made, assign it back to the PR author.


Applying
---
Things to check before applying the PR.

- Check if the comment of the PR has an `fixes ...` comment.
- Check which documentation has to be done. Preferred to fix the documentation just before the merge of the PR>
- Check for related issues and PR's
- Double check binary backwardscompatiblity.
- Add current milestone.

Build Pipeline 
===

Requirements

- Silverlight
- Xamarin Studio license for Xamarin.iOs & Xamarin.Android
- UWP (Univeral Windows platform)
- .Net 3.5 / 4.5 (you have .Net 4 with the latest .Net 4.x release)

Nuget packages and build are created in the following steps:


1. Call MSbuild with correct version numbers.  See below
2. Assemblies are patched by MSbuild
3. Binaries are written to build\bin\debug \ build\bin\release
4. NuGet packages are created from the binaries and src\NuGet\NLog\NLog.nuspec. Nuget packages are written to build\bin\release\NuGetPackages \ build\bin\debug\NuGetPackages



NuGet package management
===


## Create NuGet packages

### Example 4.1.2 RC

```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2.443 /p:BuildVersion=4.1.2-rc /p:configuration=release /p:BuildLabelOverride=NONE
```

### Example 4.1.2 (RTM)

```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2.444 /p:BuildVersion=4.1.2 /p:configuration=release /p:BuildLabelOverride=NONE
```

## Publish symbols packages

To www.symbolsource.org

```
nuget push NLog\build\bin\release\NuGetPackages\NLog.4.2.0.symbols.nupkg
nuget push NLog\build\bin\release\NuGetPackages\NLog.Extended.4.2.0.symbols.nupkg
```

## Versions

- "BuildLastMajorVersion" should be `major.0.0`. In NLog 4.x - 4.y: 4.0.0
- "AssemblyFileVersion" should be: `major.minor.patch.appVeyorBuildVersion`, eg. 4.2.2.1251 for NLog 4.2.2
- "BuildVersion" should be: `major.minor.patch` where `.patch` is ommited when 0. E.g 4.0, 4.1, 4.1.1, 4.2

Example of correct version numbers in NuGet Package explorer:

![image](https://cloud.githubusercontent.com/assets/5808377/11546997/fbfad58a-9950-11e5-952d-f7369f747089.png)




