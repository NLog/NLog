@ECHO OFF

IF NOT EXIST "%DXROOT%Data\Reflection\*.xml" GOTO BuildData

ECHO *
ECHO * %DXROOT%Data\Reflection already exists.
ECHO *
ECHO * With the October 2007 release and on, it is not necessary to create the
ECHO * files unless you install a newer version of the .NET Framework.  If
ECHO * you want to recreate it, you must delete the folder first.
ECHO *
PAUSE
GOTO Done

:BuildData
ECHO *
ECHO * This will build the Sandcastle refelection data prior to first use.
ECHO * This may take up to 30 minutes so please be patient.  Hit any key
ECHO * to start or Ctrl+C to cancel.
ECHO *
ECHO * NOTE: If running this on Vista, you may need to edit the shortcut that
ECHO *       started this script to give it Run as Administrator priviliges.
ECHO *
PAUSE

REM Determine the most recent .NET version installed
SET DNVER=2.0

IF EXIST "%WINDIR%\Microsoft.NET\Framework\v3.0\*.*" SET DNVER=3.0
IF EXIST "%WINDIR%\Microsoft.NET\Framework\v3.5\*.*" SET DNVER=3.5

"%WINDIR%\Microsoft.Net\Framework\v3.5\msbuild.exe" "%DXROOT%Examples\Sandcastle\fxReflection.proj" /Property:NetfxVer=%DNVER% /Property:PresentationStyle=vs2005

REM Clean up by removing some unnecessary stuff
RD /S /Q "%DXROOT%Data\Tmp"

del "%DXROOT%Data\Reflection\AdoNetDiag.xml"
del "%DXROOT%Data\Reflection\alink.xml"
del "%DXROOT%Data\Reflection\aspnet_filter.xml"
del "%DXROOT%Data\Reflection\aspnet_isapi.xml"
del "%DXROOT%Data\Reflection\Aspnet_perf.xml"
del "%DXROOT%Data\Reflection\aspnet_rc.xml"
del "%DXROOT%Data\Reflection\CORPerfMonExt.xml"
del "%DXROOT%Data\Reflection\cscomp.xml"
del "%DXROOT%Data\Reflection\Culture.xml"
del "%DXROOT%Data\Reflection\dfdll.xml"
del "%DXROOT%Data\Reflection\diasymreader.xml"
del "%DXROOT%Data\Reflection\EventLogMessages.xml"
del "%DXROOT%Data\Reflection\fusion.xml"
del "%DXROOT%Data\Reflection\InstallUtilLib.xml"
del "%DXROOT%Data\Reflection\MmcAspExt.xml"
del "%DXROOT%Data\Reflection\mscordacwks.xml"
del "%DXROOT%Data\Reflection\mscordbc.xml"
del "%DXROOT%Data\Reflection\mscordbi.xml"
del "%DXROOT%Data\Reflection\mscorie.xml"
del "%DXROOT%Data\Reflection\mscorjit.xml"
del "%DXROOT%Data\Reflection\mscorld.xml"
del "%DXROOT%Data\Reflection\mscorpe.xml"
del "%DXROOT%Data\Reflection\mscorrc.xml"
del "%DXROOT%Data\Reflection\mscorsec.xml"
del "%DXROOT%Data\Reflection\mscorsn.xml"
del "%DXROOT%Data\Reflection\mscorsvc.xml"
del "%DXROOT%Data\Reflection\mscortim.xml"
del "%DXROOT%Data\Reflection\mscorwks.xml"
del "%DXROOT%Data\Reflection\normalization.xml"
del "%DXROOT%Data\Reflection\PerfCounter.xml"
del "%DXROOT%Data\Reflection\peverify.xml"
del "%DXROOT%Data\Reflection\sbscmp20_mscorlib.xml"
del "%DXROOT%Data\Reflection\shfusion.xml"
del "%DXROOT%Data\Reflection\ShFusRes.xml"
del "%DXROOT%Data\Reflection\SOS.xml"
del "%DXROOT%Data\Reflection\sysglobl.xml"
del "%DXROOT%Data\Reflection\System.EnterpriseServices.Thunk.xml"
del "%DXROOT%Data\Reflection\System.EnterpriseServices.Wrapper.xml"
del "%DXROOT%Data\Reflection\TLBREF.xml"
del "%DXROOT%Data\Reflection\vjsc.xml"
del "%DXROOT%Data\Reflection\vjsnativ.xml"
del "%DXROOT%Data\Reflection\VsaVb7rt.xml"
del "%DXROOT%Data\Reflection\webengine.xml"
del "%DXROOT%Data\Reflection\WMINet_Utils.xml"

del "%DXROOT%Data\Reflection\HtmlLite.xml"
del "%DXROOT%Data\Reflection\NaturalLanguage6.xml"
del "%DXROOT%Data\Reflection\NlsData0009.xml"
del "%DXROOT%Data\Reflection\NlsLexicons0009.xml"
del "%DXROOT%Data\Reflection\PenIMC.xml"
del "%DXROOT%Data\Reflection\PresentationHostDLL.xml"
del "%DXROOT%Data\Reflection\SITSetup.xml"
del "%DXROOT%Data\Reflection\ServiceModelEvents.xml"
del "%DXROOT%Data\Reflection\ServiceMonikerSupport.xml"
del "%DXROOT%Data\Reflection\WapRes.1025.xml"
del "%DXROOT%Data\Reflection\WapRes.1028.xml"
del "%DXROOT%Data\Reflection\WapRes.1029.xml"
del "%DXROOT%Data\Reflection\WapRes.1030.xml"
del "%DXROOT%Data\Reflection\WapRes.1031.xml"
del "%DXROOT%Data\Reflection\WapRes.1032.xml"
del "%DXROOT%Data\Reflection\WapRes.1035.xml"
del "%DXROOT%Data\Reflection\WapRes.1036.xml"
del "%DXROOT%Data\Reflection\WapRes.1037.xml"
del "%DXROOT%Data\Reflection\WapRes.1038.xml"
del "%DXROOT%Data\Reflection\WapRes.1040.xml"
del "%DXROOT%Data\Reflection\WapRes.1041.xml"
del "%DXROOT%Data\Reflection\WapRes.1042.xml"
del "%DXROOT%Data\Reflection\WapRes.1043.xml"
del "%DXROOT%Data\Reflection\WapRes.1044.xml"
del "%DXROOT%Data\Reflection\WapRes.1045.xml"
del "%DXROOT%Data\Reflection\WapRes.1046.xml"
del "%DXROOT%Data\Reflection\WapRes.1049.xml"
del "%DXROOT%Data\Reflection\WapRes.1053.xml"
del "%DXROOT%Data\Reflection\WapRes.1055.xml"
del "%DXROOT%Data\Reflection\WapRes.2052.xml"
del "%DXROOT%Data\Reflection\WapRes.2070.xml"
del "%DXROOT%Data\Reflection\WapRes.3082.xml"
del "%DXROOT%Data\Reflection\WapRes.xml"
del "%DXROOT%Data\Reflection\WapUI.xml"
del "%DXROOT%Data\Reflection\dlmgr.xml"
del "%DXROOT%Data\Reflection\gencomp.xml"
del "%DXROOT%Data\Reflection\install.res.1033.xml"
del "%DXROOT%Data\Reflection\setupres.1025.xml"
del "%DXROOT%Data\Reflection\setupres.1028.xml"
del "%DXROOT%Data\Reflection\setupres.1029.xml"
del "%DXROOT%Data\Reflection\setupres.1030.xml"
del "%DXROOT%Data\Reflection\setupres.1031.xml"
del "%DXROOT%Data\Reflection\setupres.1032.xml"
del "%DXROOT%Data\Reflection\setupres.1035.xml"
del "%DXROOT%Data\Reflection\setupres.1036.xml"
del "%DXROOT%Data\Reflection\setupres.1037.xml"
del "%DXROOT%Data\Reflection\setupres.1038.xml"
del "%DXROOT%Data\Reflection\setupres.1040.xml"
del "%DXROOT%Data\Reflection\setupres.1041.xml"
del "%DXROOT%Data\Reflection\setupres.1042.xml"
del "%DXROOT%Data\Reflection\setupres.1043.xml"
del "%DXROOT%Data\Reflection\setupres.1044.xml"
del "%DXROOT%Data\Reflection\setupres.1045.xml"
del "%DXROOT%Data\Reflection\setupres.1046.xml"
del "%DXROOT%Data\Reflection\setupres.1049.xml"
del "%DXROOT%Data\Reflection\setupres.1053.xml"
del "%DXROOT%Data\Reflection\setupres.1055.xml"
del "%DXROOT%Data\Reflection\setupres.2052.xml"
del "%DXROOT%Data\Reflection\setupres.2070.xml"
del "%DXROOT%Data\Reflection\setupres.3082.xml"
del "%DXROOT%Data\Reflection\setupres.xml"
del "%DXROOT%Data\Reflection\vs70uimgr.xml"
del "%DXROOT%Data\Reflection\vs_setup.xml"
del "%DXROOT%Data\Reflection\vsbasereqs.xml"
del "%DXROOT%Data\Reflection\vsscenario.xml"

ECHO *
ECHO * The reflection data has been built.  Hit any key to exit.
ECHO *
PAUSE

:Done
