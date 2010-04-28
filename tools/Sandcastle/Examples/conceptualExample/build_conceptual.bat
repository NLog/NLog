REM ********** Set path for .net framework2.0, sandcastle,hhc,hxcomp****************************

set PATH=%windir%\Microsoft.NET\Framework\v2.0.50727;%DXROOT%\ProductionTools;%ProgramFiles%\HTML Help Workshop;%PATH%

if exist output rmdir output /s /q
if exist chm rmdir chm /s /q


XslTransform /xsl:"%DXROOT%\ProductionTransforms\dsmanifesttomanifest.xsl" aspnet_howto.buildmanifest.proj.xml /out:manifest.xml

XslTransform /xsl:"%DXROOT%\ProductionTransforms\dstoctotoc.xsl" extractedfiles\aspnet_howto.toc.xml /out:toc.xml

call "%DXROOT%\Presentation\vs2005\copyOutput.bat"

BuildAssembler /config:conceptual.config manifest.xml

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

ChmBuilder.exe /project:test /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\test.hhp