using System;
using System.Windows.Forms;

using NLogViewer.UI;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Collections;

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

        // cannot be >9 or you have to change GetRecentFileList
        const int MaxRecentFileNames = 9;

        public static List<string> GetRecentFileList()
        {
            List<string> returnValue = new List<string>();

            using (RegistryKey recentFilesKey = SettingsKey.CreateSubKey("RecentFiles"))
            {
                ArrayList valueNames = new ArrayList(recentFilesKey.GetValueNames());
                valueNames.Sort();
                foreach (string s in valueNames)
                {
                    if (s.StartsWith("File"))
                    {
                        string rfn = (string)recentFilesKey.GetValue(s, "");
                        if (rfn == "")
                            continue;

                        if (File.Exists(rfn))
                            returnValue.Add(rfn);
                        if (returnValue.Count >= MaxRecentFileNames)
                            break;
                    }
                }
            }
            return returnValue;
        }

        public static void AddToRecentFileList(string fileName)
        {
            List<string> currentRecentFiles = GetRecentFileList();
            using (RegistryKey recentFilesKey = SettingsKey.CreateSubKey("RecentFiles"))
            {
                recentFilesKey.SetValue("File1", fileName);
                int pos = 2;
                foreach (string s in currentRecentFiles)
                {
                    if (s == fileName)
                        continue;
                    recentFilesKey.SetValue("File" + pos, s);
                    pos++;
                    if (pos > MaxRecentFileNames)
                        break;
                }
            }
        }
	}
}
