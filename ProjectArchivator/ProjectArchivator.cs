using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProjectArchivator
{
    public class ProjectArchivator
    {
        ProjectArchivatorConfig sec = ProjectArchivatorConfig.GetConfig();
        private static object syncObject = new object();
        private static ProjectArchivator instance = null;

        private ProjectArchivator()
        {

        }

        public static ProjectArchivator Instance
        {
            get
            {
                if(instance == null){
                    lock(syncObject){
                        if(instance == null){
                            instance = new ProjectArchivator();
                        }
                    }
                }
                return instance;
            }
        }

        public void RecurseDirectories(string directory, List<string> dirs)
        {
            string[] subDirs = Directory.GetDirectories(directory);
            dirs.AddRange(subDirs);
            foreach (string subDir in subDirs)
            {
                RecurseDirectories(subDir, dirs);
            }
        }
        public int ArchiveFolder(string projectFolder, string targetFile, bool needRecurse, string[] excludeFolders, string[] excludePatterns, string[] includePatterns)
        {
            int result = -1;
            string curDir = projectFolder;
            Directory.SetCurrentDirectory(curDir);
            List<string> dirs = new List<string>();
            dirs.Add(curDir);
            if (needRecurse)
            {
                RecurseDirectories(curDir, dirs);
            }

            for (int i = dirs.Count - 1; i >= 0; i--)
            {
                dirs[i] = dirs[i].Substring(curDir.Length);
                foreach (string excludeFolder in excludeFolders)
                {
                    if (i == 1)
                    {

                    }
                    if (dirs[i].IndexOf(excludeFolder) == 0)//".svn") == 0)
                    {
                        dirs.RemoveAt(i);
                        continue;
                    }
                }
            }
            List<string> ResultRelativeDir = new List<string>();
            for (int i = 1; i < dirs.Count; i++)
            {
                foreach (string includePattern in includePatterns)
                {
                    ResultRelativeDir.Add(dirs[i] + "\\" + includePattern);
                }
            }
            foreach (string includePattern in includePatterns)
            {
                ResultRelativeDir.Add(dirs[0] + includePattern);
            }

            result = _7ZipNativeContainer.SevenZipCreateArchive(
                        0, 0,
                        targetFile,
                        ArchiveTypes._7z,
                        CompressionLevel.lvl9,
                        true,
                        true,
                        false,
                        true,
                        "myMPass",
                        ResultRelativeDir.ToArray()
                );
            return result;
        }
    }
}
