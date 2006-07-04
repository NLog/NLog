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
	public class LruManager
	{
        private RegistryKey _baseKey;
        private string _subKey;
        private bool _files;

        public LruManager(RegistryKey baseKey, string subKey, bool files)
        {
            _baseKey = baseKey;
            _subKey = subKey;
            _files = false;
        }

        // cannot be <=9
        const int MaxLruSize = 9;

        public List<string> GetList()
        {
            List<string> returnValue = new List<string>();

            using (RegistryKey recentFilesKey = _baseKey.CreateSubKey(_subKey))
            {
                ArrayList valueNames = new ArrayList(recentFilesKey.GetValueNames());
                valueNames.Sort();
                foreach (string s in valueNames)
                {
                    if (s.StartsWith("LRU"))
                    {
                        string rfn = (string)recentFilesKey.GetValue(s, "");
                        if (rfn == "")
                            continue;

                        if (!_files || File.Exists(rfn))
                            returnValue.Add(rfn);

                        if (returnValue.Count >= MaxLruSize)
                            break;
                    }
                }
            }
            return returnValue;
        }

        public void AddToList(string fileName)
        {
            List<string> currentRecentFiles = GetList();
            using (RegistryKey recentFilesKey = _baseKey.CreateSubKey(_subKey))
            {
                recentFilesKey.SetValue("LRU1", fileName);
                int pos = 2;
                foreach (string s in currentRecentFiles)
                {
                    if (s == fileName)
                        continue;
                    recentFilesKey.SetValue("LRU" + pos, s);
                    pos++;
                    if (pos > MaxLruSize)
                        break;
                }
            }
        }
	}
}
