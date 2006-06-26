using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace NLogViewer
{
    public static class LogLevelMap
    {
        private static Dictionary<string, LogLevel> _name2Level = new Dictionary<string, LogLevel>();

        static LogLevelMap()
        {
            string levelOrderFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels/LevelOrder.txt");

            using (StreamReader sr = new StreamReader(levelOrderFile))
            {
                string line;
                int ordinal = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');
                    string levelName = parts[0].Trim();
                    string colorName = parts[1].Trim();
                    string backColorName = parts[2].Trim();

                    Color backColor = ParseColor(backColorName);
                    Color color = ParseColor(colorName);

                    LogLevel level = new LogLevel(ordinal++, 
                        levelName, 
                        GlobalImageList.Instance.GetImageForLevel(levelName.ToUpper()),
                        color,
                        backColor);
                    _name2Level[levelName.ToUpper()] = level;
                    _name2Level[levelName] = level;
                    _name2Level[levelName.ToLower()] = level;
                }
            }
        }

        public static LogLevel GetLevelForName(string name)
        {
            try
            {
                return _name2Level[name];
            }
            catch (Exception ex)
            {
                return _name2Level[name] = new LogLevel(-1, name, -1, Color.Empty, Color.Empty);
                
            }
        }

        private static Color ParseColor(string txt)
        {
            txt = txt.Trim();
            if (txt.Length == 0 || txt == "Default")
                return Color.Empty;

            if (txt[0] == '#')
            {
                int r = Convert.ToInt32(txt.Substring(1, 2), 16);
                int g = Convert.ToInt32(txt.Substring(3, 2), 16);
                int b = Convert.ToInt32(txt.Substring(5, 2), 16);
                return Color.FromArgb(r, g, b);
            }
            else
            {
                return Color.FromName(txt);
            }
        }
    }
}
