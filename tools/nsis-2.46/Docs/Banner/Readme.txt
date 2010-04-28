BANNER PLUG-IN
--------------

The Banner plug-in shows a banner with customizable text. It uses the IDD_VERIFY dialog of the UI.

There are three functions - show, getWindow and destroy.

Usage
-----

Banner::show "Text to show"

[optional] Banner::getWindow

Banner::destroy

See Example.nsi for an example.

Modern UI
---------

The Modern UI has two labels on the IDD_VERIFY dialog. To change all the texts, use:

Banner::show /set 76 "Text 1 (replaces Please wait while Setup is loading...)" "Normal text"

Custom UI
---------

If you have more labels on your IDD_VERIFY dialog, you can use multiple /set parameters to change the texts.

Example:

Banner::show /set 76 "bah #1" /set 54 "bah #2" "Normal text"

The second parameter for /set is the ID of the control.

Some More Tricks
----------------

If you use /set to set the main string (IDC_STR, 1030) you can specify a different string for the window's caption and for the main string.

If you use an empty string as the main string (Banner::show "") the banner window will not show on the taskbar.

Credits
-------

A joint effort of brainsucker and kichik in honor of the messages dropped during the battle