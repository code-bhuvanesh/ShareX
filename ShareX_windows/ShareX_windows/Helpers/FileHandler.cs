using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShareX_windows.Helpers
{
    public static class FileHandler
    {
        public static string getFileName(string filePath)
        {
            string name = filePath.Substring(filePath.LastIndexOf("\\") + 1);

            if (name.Length > 30)
            {
                return name.Substring(0, 30) + ".....";
            }

            return name;
        }

        public static String getFileSize(long size)
        {

            if (size > 1069547520)
            {
                return Math.Round((size * 1.0 / 1069547520), 2).ToString() + " GB";
            }
            else if (size > 1048576)
            {
                return Math.Round((size * 1.0 / 1048576), 2).ToString() + " MB";
            }
            else if (size > 1024)
            {
                return Math.Round((size * 1.0 / 1024), 2).ToString() + " KB";
            }
            else
            {
                return size.ToString() + " B";
            }

        }
        public static OpenFileDialog openFile()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Multiselect = true;
            if (file.ShowDialog() == true)
            {
                return file;
            }
            return null;

        }

        public static string ReplaceInvalidChars(string filename)
        {
            //filename = = "=${??[$=${{??.jpg";
            string regexSearch = @"/\*:<>?|";
            //string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            String repalcedString = r.Replace(filename, "_");
            if(repalcedString == null)
            {
                return "";
            }
            return repalcedString;
        }
    }
}
