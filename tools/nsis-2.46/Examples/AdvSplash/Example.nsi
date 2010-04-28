Name "AdvSplash.dll test"

OutFile "AdvSplash Test.exe"

XPStyle on

Function .onInit
        # the plugins dir is automatically deleted when the installer exits
        InitPluginsDir
        File /oname=$PLUGINSDIR\splash.bmp "${NSISDIR}\Contrib\Graphics\Header\nsis.bmp"
        #optional
        #File /oname=$PLUGINSDIR\splash.wav "C:\myprog\sound.wav"

        MessageBox MB_OK "Fading"

        advsplash::show 1000 600 400 -1 $PLUGINSDIR\splash

        Pop $0          ; $0 has '1' if the user closed the splash screen early,
                        ; '0' if everything closed normally, and '-1' if some error occurred.

        MessageBox MB_OK "Transparency"
        File /oname=$PLUGINSDIR\splash.bmp "${NSISDIR}\Contrib\Graphics\Wizard\orange-uninstall.bmp"
        advsplash::show 2000 0 0 0x1856B1 $PLUGINSDIR\splash
        Pop $0 

        MessageBox MB_OK "Transparency/Fading"
        File /oname=$PLUGINSDIR\splash.bmp "${NSISDIR}\Contrib\Graphics\Wizard\llama.bmp"
        advsplash::show 1000 600 400 0x04025C $PLUGINSDIR\splash
        Pop $0 

        Delete $PLUGINSDIR\splash.bmp
FunctionEnd

Section
SectionEnd