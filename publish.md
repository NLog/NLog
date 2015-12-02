- "BuildLastMajorVersion" should be `major.0.0`. In NLog 4.x - 4.y: 4.0.0
- "AssemblyFileVersion" should be: `major.minor.patch.appVeyorBuildVersion`, eg. 4.2.2.1251 for NLog 4.2.2
- "BuildVersion" should be: `major.minor.patch` where `.patch` is ommited when 0. E.g 4.0, 4.1, 4.1.1, 4.2


Create NuGet packages
---

Example 4.1.2 RC
===


```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2.443 /p:BuildVersion=4.1.2-rc /p:configuration=release /p:BuildLabelOverride=NONE
```


Example 4.1.2 (RTM)

```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2.444 /p:BuildVersion=4.1.2 /p:configuration=release /p:BuildLabelOverride=NONE
```


Publish symbols packages
---
To www.symbolsource.org


```
nuget push NLog\build\bin\release\NuGetPackages\NLog.4.2.0.symbols.nupkg
nuget push NLog\build\bin\release\NuGetPackages\NLog.Extended.4.2.0.symbols.nupkg
```


NuGet Package explorer
---
Example of correct version numbers:

![image](https://cloud.githubusercontent.com/assets/5808377/11546997/fbfad58a-9950-11e5-952d-f7369f747089.png)
