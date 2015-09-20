

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
