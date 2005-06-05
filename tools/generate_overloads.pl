@levels = ('Log', 'Debug','Info','Warn','Error','Fatal');
@clitypes = ('System.Boolean','System.Char','System.Byte','System.String','System.Int32','System.Int64','System.Single','System.Double','System.Decimal','System.Object');
@nonclstypes = ('System.SByte','System.UInt32','System.UInt64');

for $level (@levels) {

    if ($level eq "Log") {
        $level2 = "level";
        $level3 = "specified";
        $isenabled = "IsEnabled(level)";
        $arg0 = "LogLevel level, ";
        $param0 = qq!\n		/// <param name="level">the log level.</param>!;
    } else {
        $level2 = "LogLevel.$level";
        $level3 = "<c>$level</c>";
        $isenabled = "Is${level}Enabled";
        $arg0 = "";
        $param0 = "";
    }
    

    print <<EOT;

        #region $level() overloads 

        /// <overloads>
		/// Writes the diagnostic message at the $level3 level using the specified format provider and format parameters.
        /// </overloads>
		/// <summary>
		/// Writes the diagnostic message at the $level3 level.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> to be written.</param>
        public void $level(${arg0}string message) {
            if ($isenabled)
                WriteToTargets($level2, message);
        }

		/// <summary>
		/// Writes the diagnostic message and exception at the $level3 level.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> to be written.</param>
		/// <param name="exception">An exception to be logged.</param>
        public void ${level}Exception(${arg0}string message, Exception exception) {
            if ($isenabled)
                WriteToTargets($level2, null, message, null, exception);
        }

		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>$param0
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
        public void $level(${arg0}IFormatProvider formatProvider, string message, params object[] args) { 
            if ($isenabled)
                WriteToTargets($level2, formatProvider, message, args, null); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
        public void $level(${arg0}string message, params object[] args) { 
            if ($isenabled)
                WriteToTargets($level2, message, args);
        }
        
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="arg1">First argument to format.</param>
		/// <param name="arg2">Second argument to format.</param>
        public void $level(${arg0}string message, System.Object arg1, System.Object arg2) { 
            if ($isenabled)
                WriteToTargets($level2, message, new object[] { arg1, arg2 });
        }
        
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="arg1">First argument to format.</param>
		/// <param name="arg2">Second argument to format.</param>
		/// <param name="arg3">Third argument to format.</param>
        public void $level(${arg0}string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if ($isenabled)
                WriteToTargets($level2, message, new object[] { arg1, arg2, arg3 });
        }
EOT
    for $t (@clitypes) {
        print <<EOT;
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter and formatting it with the supplied format provider.
		/// </summary>$param0
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        public void $level(${arg0}IFormatProvider formatProvider, string message, $t argument) { 
            if ($isenabled)
                WriteToTargets($level2, formatProvider, message, new object[] { argument }, null); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        public void $level(${arg0}string message, $t argument) { 
            if ($isenabled)
                WriteToTargets($level2, message, new object[] { argument });
        }
EOT
    }
    for $t (@nonclstypes) {
    print <<EOT;
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter and formatting it with the supplied format provider.
		/// </summary>$param0
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        [CLSCompliant(false)]
        public void $level(${arg0}IFormatProvider formatProvider, string message, $t argument) { 
            if ($isenabled)
                WriteToTargets($level2, formatProvider, message, new object[] { argument }, null); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        [CLSCompliant(false)]
        public void $level(${arg0}string message, $t argument) { 
            if ($isenabled)
                WriteToTargets($level2, message, new object[] { argument });
        }
EOT
    }

    print <<EOT;

        #endregion

EOT


}
