@if {%1} == {prototype} goto CheckParam2
@if {%1} == {hana} goto CheckParam2
@if {%1} == {vs2005} goto CheckParam2
@echo please specify doc style, it should be either prototype/vs2005/hana
goto End

:CheckParam2
@if {%2} == {} (
@echo please specify assembly name
goto End
)



REM ********** Set path for .net framework2.0, sandcastle,hhc,hxcomp****************************

set PATH=%windir%\Microsoft.NET\Framework\v2.0.50727;%DXROOT%\ProductionTools;%PATH%
set TOOLSPATH=%ProgramFiles%
if exist "%ProgramFiles% (x86)" set TOOLSPATH=%ProgramFiles(x86)%
set PATH=%TOOLSPATH%\HTML Help Workshop;%TOOLSPATH%\Microsoft Help 2.0 SDK;%PATH%

if exist output rmdir output /s /q
if exist chm rmdir chm /s /q

REM ********** generate reflection data files for .net framework2.0****************************
::msbuild fxReflection.proj /Property:NetfxVer=2.0 /Property:PresentationStyle=%1

REM ********** Compile source files ****************************

::csc /t:library /doc:comments.xml test.cs
::if there are more than one file, please use [ csc /t:library /doc:comments.xml *.cs ]

if exist %2.xml copy /y %2.xml comments.xml

REM ********** Call MRefBuilder ****************************

MRefBuilder %2.dll /out:reflection.org

REM ********** Apply Transforms ****************************

if {%1} == {vs2005} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=true /arg:IncludeInheritedOverloadTopics=true
) else if {%1} == {hana} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=false /arg:IncludeInheritedOverloadTopics=true
 ) else (
 XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyPrototypeDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddGuidFilenames.xsl" /out:reflection.xml 
)


XslTransform /xsl:"%DXROOT%\ProductionTransforms\ReflectionToManifest.xsl"  reflection.xml /out:manifest.xml

call "%DXROOT%\Presentation\%1\copyOutput.bat"

REM ********** Call BuildAssembler ****************************
BuildAssembler /config:"%DXROOT%\Presentation\%1\configuration\sandcastle.config" manifest.xml

REM **************Generate an intermediate Toc file that simulates the Whidbey TOC format.

if {%1} == {prototype} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createPrototypetoc.xsl" reflection.xml /out:toc.xml 
) else (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createvstoc.xsl" reflection.xml /out:toc.xml 
)

REM ************ Generate CHM help project ******************************

if not exist chm mkdir chm
if not exist chm\html mkdir chm\html
if not exist chm\icons mkdir chm\icons
if not exist chm\scripts mkdir chm\scripts
if not exist chm\styles mkdir chm\styles
if not exist chm\media mkdir chm\media

xcopy output\icons\* chm\icons\ /y /r
xcopy output\media\* chm\media\ /y /r
xcopy output\scripts\* chm\scripts\ /y /r
xcopy output\styles\* chm\styles\ /y /r

ChmBuilder.exe /project:%2 /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\%2.hhp


REM ************ Generate HxS help project **************************************

call "%DXROOT%\Presentation\shared\copyhavana.bat" %2

XslTransform /xsl:"%DXROOT%\ProductionTransforms\CreateHxc.xsl" toc.xml /arg:fileNamePrefix=%2 /out:Output\%2.HxC

XslTransform /xsl:"%DXROOT%\ProductionTransforms\TocToHxSContents.xsl" toc.xml /out:Output\%2.HxT

:: If you need to generate hxs, please uncomment the following line. Make sure "Microsoft Help 2.0 SDK" is installed on your machine.
::hxcomp.exe -p output\%2.hxc

:End