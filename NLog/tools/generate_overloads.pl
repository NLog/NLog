@levels = ('Log', 'Debug','Info','Warn','Error','Fatal');
@clitypes = ('bool','char','byte','string','int','long','float','double','decimal','object');
@nonclstypes = ('sbyte','uint','ulong');

for $level (@levels) {

    if ($level eq "Log") {
        $level2 = "level";
        $arg0 = "LogLevel level, ";
    } else {
        $level2 = "LogLevel.$level";
        $arg0 = "";
    }
    

    print <<EOT;

        #region $level() overloads 

        public void $level(${arg0}IFormatProvider formatProvider, string message, params object[] args) { 
            Write($level2, formatProvider, message, args); 
        }
        public void $level(${arg0}string message, params object[] args) { 
            Write($level2, null, message, args);
        }
        public void $level(${arg0}string message) {
            Write($level2, null, message, null);
        }
EOT
    for $t (@clitypes) {
        print <<EOT;
        public void $level(${arg0}IFormatProvider formatProvider, string message, $t argument) { 
            if (IsEnabled($level2))
                Write($level2, formatProvider, message, new object[] { argument } ); 
        }
        public void $level(${arg0}string message, $t argument) { 
            if (IsEnabled($level2))
                Write($level2, null, message, new object[] { argument });
        }
EOT
    }
    for $t (@nonclstypes) {
    print <<EOT;
        [CLSCompliant(false)]
        public void $level(${arg0}IFormatProvider formatProvider, string message, $t argument) { 
            if (IsEnabled($level2))
                Write($level2, formatProvider, message, new object[] { argument } ); 
        }
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
