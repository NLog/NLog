@levels = ('Log', 'Debug','Info','Warn','Error','Fatal');
@clitypes = ('System.Boolean','System.Char','System.Byte','System.String','System.Int32','System.Int64','System.Single','System.Double','System.Decimal','System.Object');
@nonclstypes = ('System.SByte','System.UInt32','System.UInt64');

for $level (@levels) {

    if ($level eq "Log") {
        $level2 = "level";
        $level3 = "specified";
        $arg0 = "LogLevel level, ";
        $param0 = qq!\n		/// <param name="level">the log level.</param>!;
    } else {
        $level2 = "LogLevel.$level";
        $level3 = "<c>$level</c>";
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
            Write($level2, null, message, null);
        }

		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters and formatting them with the supplied format provider.
		/// </summary>$param0
		/// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
        public void $level(${arg0}IFormatProvider formatProvider, string message, params object[] args) { 
            Write($level2, formatProvider, message, args); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified parameters.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing format items.</param>
		/// <param name="args">Arguments to format.</param>
        public void $level(${arg0}string message, params object[] args) { 
            Write($level2, null, message, args);
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
            if (IsEnabled($level2))
                Write($level2, formatProvider, message, new object[] { argument } ); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        public void $level(${arg0}string message, $t argument) { 
            if (IsEnabled($level2))
                Write($level2, null, message, new object[] { argument });
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
            if (IsEnabled($level2))
                Write($level2, formatProvider, message, new object[] { argument } ); 
        }
		/// <summary>
		/// Writes the diagnostic message at the $level3 level using the specified <see cref="T:$t" /> as a parameter.
		/// </summary>$param0
		/// <param name="message">A <see langword="string" /> containing one format item.</param>
		/// <param name="argument">The <see cref="T:$t" /> argument to format.</param>
        [CLSCompliant(false)]
        public void $level(${arg0}string message, $t argument) { 
            if (IsEnabled($level2))
                Write($level2, null, message, new object[] { argument });
        }
EOT
    }

    print <<EOT;

        #endregion

EOT


}
