using System;
using System.Windows.Forms;

using NLogViewer.UI;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;

namespace NLogViewer
{
	public class AppPreferences
	{
        private static RegistryKey SettingsKey = Registry.CurrentUser.CreateSubKey("Software\\NLogViewer");

        public static bool AlwaysOnTop
        {
            get { return Convert.ToBoolean(SettingsKey.GetValue("AlwaysOnTop", false)); }
            set { SettingsKey.SetValue("AlwaysOnTop", value); }
        }

        public static bool ShowWelcomeScreenOnStartup
        {
            get { return Convert.ToBoolean(SettingsKey.GetValue("ShowWelcomeScreenOnStartup", true)); }
            set { SettingsKey.SetValue("ShowWelcomeScreenOnStartup", value); }
        }

        public static Font LogMessagesFont
        {
            get
            {
                string family = Convert.ToString(SettingsKey.GetValue("LogMessageFontFamily", "Tahoma"));
                float emSize = Convert.ToSingle(SettingsKey.GetValue("LogMessageFontSize", 10));
                return new Font(family, emSize);
            }
            set
            {
                SettingsKey.SetValue("LogMessageFontFamily", value.FontFamily.Name);
                SettingsKey.SetValue("LogMessageFontSize", value.Size);
            }
        }
	}
}
