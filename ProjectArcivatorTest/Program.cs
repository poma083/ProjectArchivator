using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ProjectArchivator;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace ProjectArcivatorTest
{
    class Program
    {

        static Guid sysGuid;

        public static void ConvertIntToArray(int x, out byte[] ar)
        {
            ar = new byte[4];
            ar[3] = Convert.ToByte(x & 0xFF);
            ar[2] = Convert.ToByte((x >> 8) & 0xFF);
            ar[1] = Convert.ToByte((x >> 16) & 0xFF);
            ar[0] = Convert.ToByte((x >> 24) & 0xFF);
        }

        static void Main(string[] args)
        {
            ProjectArchivatorConfig sec = ProjectArchivatorConfig.GetConfig();

            sysGuid = new Guid(sec.SystemGuid);

            foreach (PathCfgElement elem in sec.Enveroument.Paths)
            {
                if (!Directory.Exists(elem.Value))
                {
                    Directory.CreateDirectory(elem.Value);
                }
            }


            List<string> fileImages = new List<string>();
            foreach (ImageTypeCfgElement elem in sec.ImgTypes)
            {
                fileImages.AddRange(Directory.EnumerateFiles(sec.Enveroument.Paths["images"].Value, elem.Value).ToList());
            }
            if (fileImages.Count < 1)
            {
                fileImages.Clear();
                List<Task> tl = ImageSearcher.search_png_First();
                Task.WaitAll(tl.ToArray());
                foreach (ImageTypeCfgElement elem in sec.ImgTypes)
                {
                    fileImages.AddRange(Directory.EnumerateFiles(sec.Enveroument.Paths["images"].Value, elem.Value).ToList());
                }
                if (fileImages.Count < 1)
                {
                    return;
                }
            }

            Dictionary<string, Guid> archiveNames = new Dictionary<string, Guid>();
            foreach (FilePatternCfgElement elem in sec.FilePatterns)
            {
                archiveNames.Add(elem.Value, Guid.NewGuid());

                int res = ProjectArchivator.ProjectArchivator.Instance.ArchiveFolder(
                    sec.SourceFolder,
                    sec.Enveroument.Paths["projects"].Value + "\\" + archiveNames[elem.Value].ToString() + ".7z",
                    true,
                    new string[] { ".svn" },
                    null,
                    new string[] { elem.Value }
                );
            }

            Random r = new Random(DateTime.Now.Millisecond);
            int imageNumber = r.Next(fileImages.Count);

            byte[] imageContent = null;
            using (FileStream fs = new FileStream(fileImages[imageNumber], FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                imageContent = new byte[fs.Length];
                fs.Read(imageContent, 0, (int)fs.Length);
            }
            byte[] testAarchiveContent = null;
            using (FileStream fs = new FileStream(sec.Enveroument.Paths["archives"].Value + "\\test1.7z", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                testAarchiveContent = new byte[fs.Length];
                fs.Read(testAarchiveContent, 0, (int)fs.Length);
            }

            // Сертификат получателя необходим для
            // зашифрования сообщения.
            // Читаем сертификат из файла.
            byte[] recCertData = File.ReadAllBytes(sec.Enveroument.Paths["cert"].Value + "\\" + sec.RecepientCert);
            X509Certificate2 recipientCert = new X509Certificate2();
            recipientCert.Import(recCertData);

            string targetFileName = Path.GetFileName(fileImages[imageNumber]);
            using (FileStream fs = new FileStream(sec.Enveroument.Paths["projects"].Value + "\\" + targetFileName, FileMode.Create))
            {
                fs.Write(imageContent, 0, (int)imageContent.Length);
                fs.Write(testAarchiveContent, 0, (int)testAarchiveContent.Length);
                foreach (FilePatternCfgElement elem in sec.FilePatterns)
                {
                    byte[] archiveContent = null;
                    using (FileStream fs1 = new FileStream(sec.Enveroument.Paths["projects"].Value + "\\" + archiveNames[elem.Value].ToString() + ".7z", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        archiveContent = new byte[fs1.Length];
                        fs1.Read(archiveContent, 0, (int)fs1.Length);
                    }

                    byte[] archiveContentEncrypted = X509Crypto.EncryptMsg(archiveContent, recipientCert);
                    fs.Write(archiveContentEncrypted, 0, archiveContentEncrypted.Length);

                    //byte[] archiveContent2 = X509Crypto.DecryptMsg(archiveContentEncrypted, new X509Certificate2Collection(ret));

                    fs.Write(sysGuid.ToByteArray(), 0, 16);
                    byte[] lenArray = null;
                    ConvertIntToArray(archiveContentEncrypted.Length, out lenArray);
                    fs.Write(lenArray, 0, 4);
                }
            }
        }
    }
}
