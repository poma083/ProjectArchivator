﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ProjectArchivator
{
    public enum ArchiveTypes { _7z, _gzip, _zip, _bzip2, _tar, _iso, _udf }
    public enum CompressionLevel { lvl0 = 0, lvl1 = 1, lvl2 = 2, lvl3 = 3, lvl4 = 4, lvl5 = 5, lvl6 = 6, lvl7 = 7, lvl8 = 8, lvl9 = 9 };


    public class _7ZipNativeContainer
    {
        [DllImport("7-zip32.dll", EntryPoint = "SevenZipSetOwnerWindowEx")]
        static extern bool SevenZipSetOwnerWindow(uint hwnd, uint CallBack);
        [DllImport("7-zip32.dll")]
        static extern int SevenZip(uint hwnd, byte[] s7cmd, byte[] s7ResultOutput, uint dwSize);

        private static string RemoveQuotes(string s)
        {
            return s.Trim('\"');
        }

        private static int SevenZipCommand(uint hwnd, string CommandLine, out string CommandOutput, uint MaxCommandOutputLen)
        {
            byte[] output = new byte[MaxCommandOutputLen];
            int Result = SevenZip(hwnd, Encoding.GetEncoding("windows-1251").GetBytes(CommandLine), output, MaxCommandOutputLen - 1);
            CommandOutput = Encoding.GetEncoding("windows-1251").GetString(output);
            return Result;
        }

        public static int SevenZipExtractArchive(uint hwnd, uint callbackMethod, string sourceFileName, bool isExtractFullPaths, bool isRecursive, bool hideProgress, string baseDir, string password, string FileList)
        {
            int result = 0;
            string[] flist = FileList.Split(';');

            string s7cmd;

            if (isExtractFullPaths)
            {
                s7cmd = "x";
            }
            else
            {
                s7cmd = "e";
            }

            s7cmd = s7cmd + " " + sourceFileName + " " + " -o" + "\"" + baseDir + "\"";

            if(!string.IsNullOrEmpty(password))
            {
                s7cmd += " -p" + password;
            }

            if (isRecursive)
            {
                s7cmd = s7cmd + " -r";
            }

            if (hideProgress)
            {
                s7cmd = s7cmd + " -hide";
            }
            s7cmd = s7cmd + " -y ";

            for (int i = 1; i < flist.Length; i++)
            {
                s7cmd = s7cmd + " -i";
                if (isRecursive)
                {
                    s7cmd = s7cmd + " -r";
                }
                s7cmd = s7cmd + "!\"" + RemoveQuotes(flist[i]) + "!\"";
            }

            SevenZipSetOwnerWindow(hwnd, callbackMethod);

            string s7ResultOutput = "";

            SevenZipCommand(hwnd, s7cmd, out s7ResultOutput, 32767);

            SevenZipSetOwnerWindow(hwnd, 0);

            if (s7ResultOutput.ToLower().IndexOf("operation aborted") >= 0)
            {
                result = 2;
            }
            else
            {
                if (s7ResultOutput.ToLower().IndexOf("error:") >= 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static int SevenZipCreateArchive(uint hwnd, uint callbackMethod, string targetFileName, ArchiveTypes type, CompressionLevel level, bool isSolid, bool isRecursive, bool hideProgress, bool encryptFileNames, string password, string[] filesToArchive)
        {
            int result = 0;
            //string[] flist = FileList.Split(';');

            //if (callback != 0)
            //    showProgress = false;

            string s7cmd = " a -t";
            switch (type)
            {
                case ArchiveTypes._7z:
                    {
                        s7cmd = s7cmd + "7z";
                        break;
                    }
                case ArchiveTypes._gzip:
                    {
                        s7cmd = s7cmd + "gzip";
                        break;
                    }
                case ArchiveTypes._zip:
                    {
                        s7cmd = s7cmd + "zip";
                        break;
                    }
                case ArchiveTypes._bzip2:
                    {
                        s7cmd = s7cmd + "bzip2";
                        break;
                    }
                case ArchiveTypes._tar:
                    {
                        s7cmd = s7cmd + "tar";
                        break;
                    }
                case ArchiveTypes._iso:
                    {
                        s7cmd = s7cmd + "iso";
                        break;
                    }
                case ArchiveTypes._udf:
                    {
                        s7cmd = s7cmd + "udf";
                        break;
                    }
            }

            s7cmd = s7cmd + " " + targetFileName;

            foreach (string file in filesToArchive)
            {
                s7cmd = s7cmd + " \"" + file + "\"";
            }

            s7cmd = s7cmd + " -mx" + level.ToString("d");

            if (isSolid)
            {
                s7cmd = s7cmd + " -ms=on";
            }

            //s7cmd = s7cmd + " -m0=PPMd";
            //s7cmd = s7cmd + " -w=\"C:\\Projects\\C#\\TCB Express\\onlineWeb\\\"";

            if (!String.IsNullOrEmpty(password))
            {
                s7cmd = s7cmd + " -p" + password;
            }
            if (encryptFileNames)
            {
                s7cmd = s7cmd + " -mhe";
            }

            if (isRecursive)
            {
                //s7cmd = s7cmd + " -r";
            }

            if (hideProgress)
            {
                s7cmd = s7cmd + " -hide";
            }
            //s7cmd = s7cmd + " -aoa";

            //s7cmd = " a c:\a.7z \"C:\\TEMP\\1\"";

            SevenZipSetOwnerWindow(hwnd, callbackMethod);

            string s7ResultOutput = "";

            SevenZipCommand(hwnd, s7cmd, out s7ResultOutput, 32767);

            SevenZipSetOwnerWindow(hwnd, callbackMethod);

            if (s7ResultOutput.ToLower().IndexOf("operation aborted") >= 0)
            {
                result = 2;
            }
            else
            {
                if (s7ResultOutput.ToLower().IndexOf("error:") >= 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
    }
}
