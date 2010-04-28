StartMenu.dll shows a custom page that lets the user select a start menu program 
folder to put shortcuts in.

To show the dialog use the Select function. This function has one required parameter 
which is the program group default name, and some more optional switches:
  /autoadd - automatically adds the program name to the selected folder
  /noicon - doesn't show the icon in the top left corner
  /text [please select...] - sets the top text to something else than
                             "Select the Start Menu folder in which..."
  /lastused [folder] - sets the edit box to a specific value folder.
                       Use this to make this plug-in remember the last
                       folder selected by the user
  /checknoshortcuts text - Shows a check box with the text "text". If
                           the user checks this box, the return value
                           will have > as its first character and you
                           should not create the program group.
  /rtl - sets the direction of every control on the selection dialog
         to RTL. This means every text shown on the page will be
	 justified to the right.

The order of the switches doesn't matter but the required parameter must come after
all of them. Every switch after the required parameter will be ignored and left
on the stack.

The function pushes "success", "cancel" or an error to the stack. If there was no
error and the user didn't press on cancel it will push the selected folder name
after "success". If the user checked the no shortcuts checkbox the result will be
prefixed with '>'. The function does not push the full path but only the selected
sub-folder. It's up to you to decide if to put it in the current user or all
users start menu.

To set properties of the controls on the page, such as colors and fonts use Init
and Show instead of Select. Init will push the HWND of the page on the stack,
or an error string. For example:

StartMenu::Init "Test"
Pop $0
IntCmp $0 0 failed
GetDlgItem $0 $0 1003
SetCtlColors $0 "" FF0000
StartMenu::Show
# continue as with Select here
failed:

Look at Example.nsi for a full example (without Init and Select).

Created by Amir Szekely (aka KiCHiK)