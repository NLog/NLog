using System;

namespace NLog.Targets
{
    /// <summary>
    /// Writes log messages to the console with customizable coloring
    /// The maximum decrease in the number of switching colors console
    /// x2-3 faster than ColoredConsole, but not so advanced 
    /// </summary>
    [Target("FastColoredConsole")]
    public class FastColoredConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Gets or sets a value indicating whether to send the log messages to the standard error instead of the standard output.
        /// </summary>
        public bool Error { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to use foreground color.
        /// </summary>
        public bool UseForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to use background color.
        /// </summary>
        public bool UseBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to reset color after write.
        /// </summary>
        public bool UseResetColor { get; set; }

        /// <summary>
        /// Gets or sets a value for Trace foreground color.
        /// </summary>
        public ConsoleColor TraceForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Trace background color.
        /// </summary>
        public ConsoleColor TraceBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Debug foreground color.
        /// </summary>
        public ConsoleColor DebugForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Debug background color.
        /// </summary>
        public ConsoleColor DebugBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Info foreground color.
        /// </summary>
        public ConsoleColor InfoForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Info background color.
        /// </summary>
        public ConsoleColor InfoBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Warning foreground color.
        /// </summary>
        public ConsoleColor WarningForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Warning background color.
        /// </summary>
        public ConsoleColor WarningBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Error foreground color.
        /// </summary>
        public ConsoleColor ErrorForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Error background color.
        /// </summary>
        public ConsoleColor ErrorBackgroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Fatal foreground color.
        /// </summary>
        public ConsoleColor FatalForegroundColor { get; set; }
        /// <summary>
        /// Gets or sets a value for Fatal background color.
        /// </summary>
        public ConsoleColor FatalBackgroundColor { get; set; }

        private ConsoleColor _lastForegroundColor;
        private ConsoleColor _lastBackgroundColor;

        /// <summary>
        /// ctor
        /// </summary>
        public FastColoredConsoleTarget()
        {
            _lastForegroundColor = Console.ForegroundColor;
            _lastBackgroundColor = Console.BackgroundColor;
            Error = false;
            UseForegroundColor = true;
            UseBackgroundColor = false;
            UseResetColor = false;
            TraceBackgroundColor = DebugBackgroundColor = InfoBackgroundColor =
                WarningBackgroundColor = ErrorBackgroundColor = FatalBackgroundColor = ConsoleColor.Black;
            TraceForegroundColor = ConsoleColor.DarkGray;
            DebugForegroundColor = ConsoleColor.DarkGreen;
            InfoForegroundColor = ConsoleColor.White;
            WarningForegroundColor = ConsoleColor.Magenta;
            ErrorForegroundColor = ConsoleColor.Yellow;
            FatalForegroundColor = ConsoleColor.Red;

        }
        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (Header != null)
            {
                Output(Header.Render(LogEventInfo.CreateNullEvent()), LogLevel.Info);
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                Output(Footer.Render(LogEventInfo.CreateNullEvent()), LogLevel.Info);
            }
            base.CloseTarget();
        }

        /// <summary>
        /// Writes the specified logging event to the Console.Out or
        /// Console.Error depending on the value of the Error flag.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <remarks>
        /// Note that the Error option is not supported on .NET Compact Framework.
        /// </remarks>
        protected override void Write(LogEventInfo logEvent)
        {
            Output(Layout.Render(logEvent), logEvent.Level);
        }

        private void Output(string s, LogLevel level)
        {
            if (UseForegroundColor || UseBackgroundColor)
                if (level == LogLevel.Trace)
                    SetColor(TraceForegroundColor, TraceBackgroundColor);
                else if (level == LogLevel.Debug)
                    SetColor(DebugForegroundColor, DebugBackgroundColor);
                else if (level == LogLevel.Info)
                    SetColor(InfoForegroundColor, InfoBackgroundColor);
                else if (level == LogLevel.Warn)
                    SetColor(WarningForegroundColor, WarningBackgroundColor);
                else if (level == LogLevel.Error)
                    SetColor(ErrorForegroundColor, ErrorBackgroundColor);
                else if (level == LogLevel.Fatal)
                    SetColor(FatalForegroundColor, FatalBackgroundColor);
            (Error ? Console.Error : Console.Out).WriteLine(s);
            if(UseResetColor)
                Console.ResetColor();
        }

        private void SetColor(ConsoleColor foreground, ConsoleColor background)
        {
            if (UseForegroundColor && (UseResetColor || _lastForegroundColor != foreground))
            {
                _lastForegroundColor = foreground;
                Console.ForegroundColor = foreground; 
            }
            if (UseBackgroundColor && (UseResetColor || _lastBackgroundColor != background))
            {
                _lastBackgroundColor = background;
                Console.BackgroundColor = background;
            }
        }
    }
}
