using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ProjectArchivator
{
    public class ImageSearcher
    {
        public static Dictionary<string, int> nameCount = new Dictionary<string, int>();

        public static bool Check7zFile(byte[] content)
        {
            byte[] sign = new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };
            for (int i = 0; i < content.Length; i++)
            {
                if ((content.Length - i) < 32)
                {
                    break;
                }
                if (content[i] != sign[0])
                {
                    continue;
                }
                i++;
                if (content[i] != sign[1])
                {
                    continue;
                }
                i++;
                if (content[i] != sign[2])
                {
                    continue;
                }
                i++;
                if (content[i] != sign[3])
                {
                    continue;
                }
                i++;
                if (content[i] != sign[4])
                {
                    continue;
                }
                i++;
                if (content[i] != sign[5])
                {
                    continue;
                }
                return true;
            }
            return false;
        }
        public static void CopyFiles(string sourceDirectory, string targetDirectory, string filter)
        {
            try
            {
                IEnumerable<string> fileNames = Directory.EnumerateFiles(sourceDirectory + "\\", filter);
                foreach (string file in fileNames)
                {
                    byte[] imageContent = null;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imageContent = new byte[fs.Length];
                        fs.Read(imageContent, 0, (int)fs.Length);
                    }
                    if (Check7zFile(imageContent))
                    {
                        continue;
                    }
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string fileExtention = Path.GetExtension(file);
                    lock (nameCount)
                    {
                        if (nameCount.ContainsKey(fileName + fileExtention))
                        {
                            nameCount[fileName + fileExtention]++;
                        }
                        else
                        {
                            nameCount.Add(fileName + fileExtention, 0);
                        }
                        fileName = fileName + "_(" + nameCount[fileName + fileExtention].ToString() + ")";
                    }
                    using (FileStream fs = new FileStream(targetDirectory + "\\" + fileName + fileExtention, FileMode.Create))
                    {
                        fs.Write(imageContent, 0, (int)imageContent.Length);
                    }


                }
            }
            catch(UnauthorizedAccessException ex){

            }
        }
        public static void search_image(object context)// string dirParrent, string dirTarget, string rootTarget)
        {
            string[] dirs = context as string[];
            if (dirs == null)
            {
                return;
            }
            if (dirs[0] == dirs[2])
            {
                return;
            }
            ProjectArchivatorConfig sec = ProjectArchivatorConfig.GetConfig();
            foreach (ImageTypeCfgElement elem in sec.ImgTypes)
            {
                CopyFiles(dirs[0], dirs[2], elem.Value);
            }
            try
            {
                IEnumerable<string> directories = Directory.EnumerateDirectories(dirs[0] + "\\");
                foreach (string dir in directories)
                {
                    string dirName = Path.GetFileName(dir);
                    search_image(new string[] { dirs[0] + "\\" + dirName, dirs[1] + "\\" + dirName, dirs[2] });
                }
            }
            catch (UnauthorizedAccessException ex)
            {

            }
        }
        public static List<Task> search_png_First()
        {
            ProjectArchivatorConfig sec = ProjectArchivatorConfig.GetConfig();
            
            List<Task> tl = new List<Task>();
            if (!Directory.Exists(sec.Enveroument.Paths["images"].Value))
            {
                Directory.CreateDirectory(sec.Enveroument.Paths["images"].Value);
            }
            Task ttt = new Task(search_image, new string[] { sec.Enveroument.Paths["imageFrom"].Value, sec.Enveroument.Paths["images"].Value, sec.Enveroument.Paths["images"].Value });
            tl.Add(ttt);
            ttt.Start();
            return tl;
        }
    }
}
