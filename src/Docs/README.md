# API docs generation
Generated docs for https://nlog-project.org/documentation/

## How to run
1. Install [Sandcastle Help File Builder (SHFB) ](https://github.com/EWSoftware/SHFB), tested it with [v2022.2.6.0](https://github.com/EWSoftware/SHFB/releases/tag/v2022.2.6.0)
1. Perhaps update the year. Search for `2004-` for in [NLog.shfbproj](NLog.shfbproj).
1. Start *Developer Command Prompt for VS 2022*
1. Run [BuildDoc.cmd](BuildDoc.cmd)
1. Copy the [Doc](Doc) folder to documentation folder of nlog.github.io
1. Remove unneeded files: .config, .aspx. .php, WebKI.xml, WebTOC.xml and LastBuild.log

