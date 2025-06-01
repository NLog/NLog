Support & contributing guidelines
===
Do you have feature requests, questions or would you like to report a bug? Please follow these guidelines when posting on the [issue list](https://github.com/NLog/NLog/issues).

Feature requests
----
Please provide the following information:
- The current NLog version
- Any current work-arounds
- Example of the config when implemented. Please use [fenced code blocks](https://help.github.com/articles/creating-and-highlighting-code-blocks/#fenced-code-blocks).
- Pull requests and unit tests are welcome! 

Questions
----
Please provide the following information:
- The current NLog version
- The current config (xml or code). Please use [fenced code blocks](https://help.github.com/articles/creating-and-highlighting-code-blocks/#fenced-code-blocks).
- If relevant: the current result (Including full exception details if any)
- If relevant: the expected result

Bug reports
----
Please provide the following information:
- The current NLog version
- The error message and stacktrace. Please use [fenced code blocks](https://help.github.com/articles/creating-and-highlighting-code-blocks/#fenced-code-blocks).
- The internal log, `Debug` level. See [Internal Logging](https://github.com/NLog/NLog/wiki/Internal-Logging)
- The current result
- The expected result 
- Any current work-arounds
- The current config (xml or code). Please use [fenced code blocks](https://help.github.com/articles/creating-and-highlighting-code-blocks/#fenced-code-blocks).
- Pull requests and unit tests are welcome!


Pull requests
----
Unit tests are really appreciated! 

Please document any public method and property. Document **why** and not how. At least required: 

* Method: Summary, param and return.
* Property: Summary


Multiple .NET versions
===
Keep in mind that multiple versions of .NET are supported. Some methods are not available in all .NET versions. The following conditional compilation symbols can be used:

```
#if NET35
#if NETFRAMEWORK
#if NETSTANDARD
#if NETSTANDARD2_1_OR_GREATER
```

Update your fork
===
Is your fork not up-to-date with the NLog code? Most of the time that isn't a problem. But if you like to "sync back" the changes to your repository, execute the following command:

The first time:
```
git remote add upstream https://github.com/NLog/NLog.git 
```


After that you repository will have two remotes. You could update your remote (the fork) in the following way:

```
git fetch upstream
git checkout <your feature branch>
git rebase upstream/master
..fix if needed and
git push -f 
```

if `rebase` won't work well, use `git merge master` as alternative.

It's also possible to send a PR in the opposite direction, but that's not preferred as it will pollute the commit log.


Contributing
---
As the current NLog team is a small team, we cannot fix every bug or implement every feature on our own. So contributions are really appreciated!

If you like to start with a small task, then
[up-for-grabs](https://github.com/NLog/NLog/issues?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs)  are nice to start with.

Please note, we have a `dev` and `master` branch

- `master` is for pure bug fixes and targets NLog 5.x
- `dev` targets NLog 6


A good way to get started (flow)

1. Fork the NLog repos.
2. Create a new branch in you current repos from the 'dev' branch. (critical bugfixes from 'master')
3. 'Check out' the code with Git or [GitHub Desktop](https://desktop.github.com/)
4. Push commits and create a Pull Request (PR) to NLog

Please note: bugfixes should target the **master** branch, others the **dev** branch (NLog 6)


How to build
---
Use Visual Studio 2022 and open the solution 'NLog.sln'.

For building in the cloud we use:
- AppVeyor for Windows- and Linux-builds
- SonarQube for code coverage

Trying to build your fork in the cloud? Check [this how-to](howto-build-your-fork.md)

Note: master points to NLog 5.x and dev to NLog 6
