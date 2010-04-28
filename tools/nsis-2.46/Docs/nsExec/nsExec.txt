nsExec
------
nsExec will execute command-line based programs and capture the output
without opening a dos box.


Usage
-----
nsExec::Exec [/OEM] [/TIMEOUT=x] path

-or-

nsExec::ExecToLog [/OEM] [/TIMEOUT=x] path

-or-

nsExec::ExecToStack [/OEM] [/TIMEOUT=x] path

All functions are the same except ExecToLog will print the output
to the log window and ExecToStack will push up to ${NSIS_MAX_STRLEN}
characters of output onto the stack after the return value.

Use the /OEM switch to convert the output text from OEM to ANSI.

The timeout value is optional.  The timeout is the time in
milliseconds nsExec will wait for output.  If output from the
process is received, the timeout value is reset and it will
again wait for more output using the timeout value.  See Return 
Value for how to check if there was a timeout.

To ensure that command are executed without problems on all windows versions,
is recommended to use the following syntax:

   nsExec::ExecToStack [OPTIONS] '"PATH" param1 param2 paramN'

This way the application path may contain non 8.3 paths (with spaces)

Return Value
------------
If nsExec is unable to execute the process, it will return "error"
on the top of the stack, if the process timed out it will return
"timeout", else it will return the return code from the
executed process.


Copyright Info
--------------
Copyright (c) 2002 Robert Rainwater
Thanks to Justin Frankel and Amir Szekely