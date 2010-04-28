----------------------------------------------------
MakeNSISW - MakeNSIS Windows Wrapper
----------------------------------------------------


About MakeNSISW
---------------
MakeNSISW is a wrapper for the MakeNSIS that is distributed with
NSIS (http://www.nullsoft.com/free/nsis/).  MakeNSISW allows you 
to compile NSIS scripts using a Windows GUI interface.  To install
MakeNSISW, compile the source using Visual C++ or Mingw.


Requirements
------------
MakeNSISW requires NSIS be installed on your system.  The default 
directory for this installation is $PROGRAMFILES\NSIS\Contrib\MakeNSISW.


Usage:
------
If you installed the Shell Extensions option during the installation, then
all that is required is that you choose 'Compile NSI' from the right-
click menu on a NSIS script.  This will invoke MakeNSISW.

The format of the parameters when calling MakeNSISW from the commandline is:
  makensisw [options] [script.nsi | - [...]]

For the options, please see the MakeNSIS documentation.


Shortcut Keys
-------------
Ctrl+A: Select All text
Ctrl+B: Open Script Folder
Ctrl+C: Copy selected text
Ctrl+D: Opens the Define Symbols dialog
Ctrl+E: Edits the script
Ctrl+F: Find text
Ctrl+L: Load a script
Ctrl+R: Recompiles the script
Ctrl+T: Tests the installer
Ctrl+W: Clear Log Window
Alt+X: Exits the application
F1: View Documentation


Version History
---------------
0.1
 - Initial Release

0.2
 - Added ability to save output and copy output

0.3
 - Added option to recompile script (F2 or File|Recompile)
 - Added Help Menu
 - Return code is now always set
 - Added Accelerator key support for Exit and Recompile
 - No longer uses NSIS's version string
 - Made clearer status message in title bar
 - Disabled menu/accelerator functions during compile

0.4
 - Fixed Copy Selected bug

0.5
 - Minor Makefile changes (mingw)
 - Moved strings into global strings to make editing easier
 - Added Clear Log Command under Edit menu
 - Recompile no longer clears the log window (use F5)
 - Close is now the default button when you hit enter
 - added VC++ project, updated resources to work with VC++
 - rearranged directory structure
 - makefiles now target ../../makensisw.exe
 - removed makensisw home link in help menu (hope this is ok,
   doesn't really seem needed to me)
 - made display use a fixed width font (Some people may not like
   this, but I do)
 - added 'test' button (peeks output for 'Output' line)
 - made it so that the log shows the most recent 32k.
 - made it so that the log always clears on a recompile.
 - compiled with VC++ so no longer needs msvcrt.dll
 - made the compiler name be a full path (for more flexibility)

0.6
 - print correct usage if unable to execute compiler
 - removed mingw warnings
 - set title/branding before errors
 - some docs changes
 - Added Edit|Edit Script function

0.7
 - Edit Script should now work for output>32k
 - Added resize support (thanks to felfert)
 - Added window position saving (thanks to felfert)
 - Disable some items when exec of makensis failed

0.8
 - Added window size constraints (thanks to bcheck)
 - Cleaned up the resource file

0.9
 - Removed global strings (moved into #defines)
 - Some GUI changes
 - No longer focused Close button (its default anyways)
 - Fixed resize bug on minimize/restore (thanks to felfert)
 - Made window placement stored in HKLM instead of HKCU, cause
   I hate things that get littered in HKCU.

1.0
 - Fixed bug with large output causing crash

1.1
 - Crash may actually be fixed

1.2
 - XP visual style support

1.3
 - Added Documentation menu item
 - Fix GUI problem with About dialog

1.4
 - Edit Script command will now work with or without file associations
 - Added default filename for save dialog
 - Use standard fonts
 - Documentation menuitem caused recompile

1.5
 - Fixed Copy All function

1.6
 - Reduced size from 44k to 12k (kichik)
 - Editbox not limited to 32k (now using richedit control)
 - Made the log window font-size smaller.

1.7
 - Added check for warnings
 - Added sound for sucessfull compilations
 - Update home page and documentation menu items to Sourceforge page

1.8
 - Contents of log window are now streamed in
 - Empty log window check (to prevent random crashes)

1.9
 - Text always scrolls to bottom (kichik)
 - Updated link to new docs
 - Makensisw now takes the same parameters as makensis.exe
 - Fixed some random crashes
 - Drag and Drop Support into the Makensisw window
 - Updated icon to more sexy one
 - Added Load Script option on File menu
 - Added Search Dialog (Ctrl+F) (kichik)
 - Added Select All (Ctrl+A), Copy (Ctrl+C), Exit (Alt+X) keys
 - Branding text now reflects NSIS version
 - Added some simple tool tips
 - Added Context Menu in log window
 - Added resize gripper
 - Ctrl+L loads a script
 - Added Clear Log (Ctrl+W)
 - Browse Script (Ctrl+B) launches explorer in script directory
 - Check for Update command
 - Added link to the NSIS Forum under Help menu
 - Bunch of other stuff not worth mentioning
 - Define Symbols menu (Ctrl+D)

2.0
 - Improved user interface
 - Define Symbols is available even if a script is not loaded
 - Defined Symbols are saved on exit and reloaded on start
 - Added NSIS Update menu
 - Added toolbar for commonly used menus
 - Made the Toolbar style flat
 - Added option for compile & run
 - Added compressor setting option
 - Added support for lzma compression
 - Added named Symbols sets.

2.1
 - Added "Cancel compilation" menu item

2.2
- Settings saved in HKCU instead of HKLM
- Added menu accelerators to MRU list

2.3
- Escape button closes MakeNSISw

2.3.1
- Fixed broken command line parameter handling
 
Copyright Information
---------------------
Copyright (c) 2002 Robert Rainwater
Contributors: Justin Frankel, Fritz Elfert, Amir Szekely, Sunil Kamath, Joost Verburg

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
