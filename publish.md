Create NuGet packages
---

Example 4.1.2 RC
===


```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2.0 /p:BuildVersion=4.1.2-rc /p:configuration=release /p:BuildLabelOverride=NONE
```


Example 4.1.2 (RTM)

```
cd nlog/src
msbuild NLog.proj /t:rebuild /t:NuGetPackage  /p:BuildLastMajorVersion=4.0.0 /p:AssemblyFileVersion=4.1.2 /p:BuildVersion=4.1.2 /p:configuration=release /p:BuildLabelOverride=NONE
```


Publish symbols packages
---
To www.symbolsource.org


```
nuget push NLog\build\bin\release\NuGetPackages\NLog.4.2.0.symbols.nupkg
nuget push NLog\build\bin\release\NuGetPackages\NLog.Extended.4.2.0.symbols.nupkg
```
