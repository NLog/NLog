Support & contributing guidelines (Oktober 13, 2015)
===
Do you have feature requests, questions or would you like to report a bug? Please follow these guidelines when posting on the [issue list](https://github.com/NLog/NLog/issues). The issues are labeled with the [following guideline](issue-labeling.md). 

Feature requests
----
Please provide the following information:
- The current NLog version
- Any current work-arounds
- Example of the config when implemented. Please use [fenced code blocks](https://help.github.com/articles/github-flavored-markdown/#fenced-code-blocks).
- Pull requests and unit tests are welcome! 

Questions
----
Please provide the following information:
- The current NLog version
- The current config (file content or API calls). Please use [fenced code blocks](https://help.github.com/articles/github-flavored-markdown/#fenced-code-blocks).
- If relevant: the current result
- If relevant: the expected result

 

Bug reports
----
Please provide the following information:
- The current NLog version
- The error message and stacktrace. Please use [fenced code blocks](https://help.github.com/articles/github-flavored-markdown/#fenced-code-blocks).
- The internal log, `Debug` level. See [Internal Logging](https://github.com/NLog/NLog/wiki/Internal-Logging)
- The current result
- The expected result 
- Any current work-arounds
- The current config (file content or API calls). Please use [fenced code blocks](https://help.github.com/articles/github-flavored-markdown/#fenced-code-blocks).
- Pull requests and unit tests are welcome!



Pull requests
----
Unit tests are really appreciated! Also please [Sync all the projects](#Sync projects) 

Please document any public method and property. Document **why** and not how. At least required: 

* Method: Summary, param and return.
* Property: Summary

Sync projects
===
Adding or removing files to the project? Please keep all project files in sync, otherwise AppVeyor will complain.
The following Msbuild command can be used:
```
NLog\src>msbuild NLog.proj /t:SyncProjectItems
```

MSbuild is located in:
```
"C:\Windows\Microsoft.NET\Framework\v...\MSBuild.exe"
```


Multiple .Net versions
===
Keep in mind that multiple versions of .Net are supported. Some methods are not available in all .Net versions. The following conditional compilation symbols can be used:

```
#if NET3_5
#if NET4_0
#if NET4_5
#if SILVERLIGHT
#if SILVERLIGHT5
#if #MONO
#if #MONO_2_0
#if #WINDOWS_PHONE
#if #WINDOWS_PHONE_7
#if #WINDOWS_PHONE_7_1
```

Sync back
===
Is your fork not up-to-date with the NLog code? Most of the time that isn't a problem. But I you like to "sync back" the changes to your repository, execute the following command:

The first time:
```
git remote add upstream https://github.com/NLog/NLog.git 
```

After that:

```
git fetch upstream
git checkout <your feature branch>
git rebase upstream/master
..fix if needed and
git push -f 
```

if `rebase` wont work well, alternative use `git merge master`

It's also possible to send a PR in the opposite direction, but that's not preferred as it will pollute the commit log.
