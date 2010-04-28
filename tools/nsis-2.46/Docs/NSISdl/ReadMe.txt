NSISdl 1.3 - HTTP downloading plugin for NSIS
---------------------------------------------

Copyright (C) 2001-2002 Yaroslav Faybishenko & Justin Frankel

This plugin can be used from NSIS to download files via http.

To connect to the internet, use the Dialer plugin.

USAGE
-----

NSISdl::download http://www.domain.com/file localfile.exe

You can also pass /TIMEOUT to set the timeout in milliseconds:

NSISdl::download /TIMEOUT=30000 http://www.domain.com/file localfile.exe

The return value is pushed to the stack:

  "cancel" if cancelled
  "success" if success
  otherwise, an error string describing the error

If you don't want the progress window to appear, use NSISdl::download_quiet.

Example of usage:

NSISdl::download http://www.domain.com/file localfile.exe
Pop $R0 ;Get the return value
  StrCmp $R0 "success" +3
    MessageBox MB_OK "Download failed: $R0"
    Quit

For another example, see waplugin.nsi in the examples directory.

PROXIES
-------

NSISdl supports only basic configurations of proxies. It doesn't support
proxies which require authentication, automatic configuration script, etc.
NSISdl reads the proxy configuration from Internet Explorer's registry key
under HKLM\Software\Microsoft\Windows\CurrentVersion\Internet Settings. It
reads and parses ProxyEnable and ProxyServer.

If you don't want NSISdl to use Internet Explorer's settings, use the
/NOIEPROXY flag. /NOIEPROXY should be used after /TRANSLATE and
/TIMEOUT. For example:

If you want to specify a proxy on your own, use the /PROXY flag.

NSISdl::download /NOIEPROXY http://www.domain.com/file localfile.exe
NSISdl::download /TIMEOUT=30000 /NOIEPROXY http://www.domain.com/file localfile.exe
NSISdl::download /PROXY proxy.whatever.com http://www.domain.com/file localfile.exe
NSISdl::download /PROXY proxy.whatever.com:8080 http://www.domain.com/file localfile.exe

TRANSLATE
---------

To translate NSISdl add the following values to the call line:

/TRANSLATE2 downloading connecting second minute hour seconds minutes hours progress

Default values are:
 
  downloading - "Downloading %s"
  connecting - "Connecting ..."
  second - " (1 second remaining)"
  minute - " (1 minute remaining)"
  hour - " (1 hour remaining)"
  seconds - " (%u seconds remaining)"
  minutes - " (%u minutes remaining)"
  hours - " (%u hours remaining)"
  progress - "%skB (%d%%) of %skB @ %u.%01ukB/s"

The old /TRANSLATE method still works for backward compatibility.

/TRANSLATE downloading connecting second minute hour plural progress remianing

Default values are:

  downloading - "Downloading %s"
  connecting - "Connecting ..."
  second - "second"
  minute - "minute"
  hour - "hour"
  plural - "s"
  progress - "%dkB (%d%%) of %ukB @ %d.%01dkB/s"
  remaining -  " (%d %s%s remaining)"

/TRANSLATE and /TRANSLATE2 must come before /TIMEOUT.
