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

        static List<string> Init(ProjectArchivatorConfig cfg)
        {
            sysGuid = new Guid(cfg.SystemGuid);

            foreach (PathCfgElement elem in cfg.Enveroument.Paths)
            {
                if (!Directory.Exists(elem.Value))
                {
                    Directory.CreateDirectory(elem.Value);
                }
            }
            if (!File.Exists(cfg.Enveroument.Paths["archives"].Value + "\\" + cfg.SystemGuid + ".7z"))
            {
                using (FileStream fs = new FileStream(cfg.Enveroument.Paths["archives"].Value + "\\" + cfg.SystemGuid + ".txt", FileMode.Create))
                {
                    fs.Write(Encoding.UTF8.GetBytes("hello world!!!"), 0, (int)Encoding.UTF8.GetBytes("hello world!!!").Length);
                }
                int res = ProjectArchivator.ProjectArchivator.Instance.ArchiveFolder(
                    cfg.Enveroument.Paths["archives"].Value,
                    cfg.Enveroument.Paths["archives"].Value + "\\" + cfg.SystemGuid + ".7z",
                    true,
                    new string[] { },
                    null,
                    new string[] { cfg.SystemGuid + ".txt" }
                );
                File.Delete(cfg.Enveroument.Paths["archives"].Value + "\\" + cfg.SystemGuid + ".txt");
            }

            List<string> fileImages = new List<string>();
            foreach (ImageTypeCfgElement elem in cfg.ImgTypes)
            {
                fileImages.AddRange(Directory.EnumerateFiles(cfg.Enveroument.Paths["images"].Value, elem.Value).ToList());
            }
            if (fileImages.Count < 1)
            {
                throw new Exception("Накидайте в папку images каких-нибудь картинок");
                //fileImages.Clear();
                //List<Task> tl = ImageSearcher.search_png_First();
                //Task.WaitAll(tl.ToArray());
                //foreach (ImageTypeCfgElement elem in sec.ImgTypes)
                //{
                //    fileImages.AddRange(Directory.EnumerateFiles(cfg.Enveroument.Paths["images"].Value, elem.Value).ToList());
                //}
                //if (fileImages.Count < 1)
                //{
                //    return;
                //}
            }
            
            return fileImages;
        }

        static void Main(string[] args)
        {
            ProjectArchivatorConfig cfg = ProjectArchivatorConfig.GetConfig();
            List<string> fileImages = Init(cfg);

            Dictionary<string, Guid> archiveNames = new Dictionary<string, Guid>();
            foreach (FilePatternCfgElement elem in cfg.FilePatterns)
            {
                archiveNames.Add(elem.Value, Guid.NewGuid());

                int res = ProjectArchivator.ProjectArchivator.Instance.ArchiveFolder(
                    cfg.Source,
                    cfg.Enveroument.Paths["projects"].Value + "\\" + archiveNames[elem.Value].ToString() + ".7z",
                    true,
                    new string[] { ".svn" },
                    null,
                    new string[] { elem.Value }
                );
            }

            Random r = new Random(DateTime.Now.Millisecond);
            int imageNumber = r.Next(fileImages.Count);

            byte[] imageContent = null;
            using (FileStream fs = new FileStream(fileImages[imageNumber], FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                imageContent = new byte[fs.Length];
                fs.Read(imageContent, 0, (int)fs.Length);
                if (!ImageSearcher.Check7zFile(imageContent))
                {
                    byte[] testAarchiveContent;
                    using (FileStream fsAC = new FileStream(cfg.Enveroument.Paths["archives"].Value + "\\" + cfg.SystemGuid + ".7z", FileMode.Open))
                    {
                        testAarchiveContent = new byte[fsAC.Length];
                        fsAC.Read(testAarchiveContent, 0, testAarchiveContent.Length);
                    }
                    fs.Write(testAarchiveContent, 0, testAarchiveContent.Length);
                    Array.Resize<byte>(ref imageContent, imageContent.Length + testAarchiveContent.Length);
                    Array.Copy(testAarchiveContent, 0, imageContent, imageContent.Length - testAarchiveContent.Length, testAarchiveContent.Length);
                }
            }

            // Сертификат получателя необходим для
            // зашифрования сообщения.
            // Читаем сертификат из файла.
            byte[] recCertData = File.ReadAllBytes(cfg.Enveroument.Paths["cert"].Value + "\\" + cfg.RecepientCert);
            X509Certificate2 recipientCert = new X509Certificate2();
            recipientCert.Import(recCertData);

            string targetFileName = Path.GetFileName(fileImages[imageNumber]);
            using (FileStream fs = new FileStream(cfg.Enveroument.Paths["projects"].Value + "\\" + targetFileName, FileMode.Create))
            {
                fs.Write(imageContent, 0, (int)imageContent.Length);
                foreach (FilePatternCfgElement elem in cfg.FilePatterns)
                {
                    byte[] archiveContent = null;
                    using (FileStream fs1 = new FileStream(cfg.Enveroument.Paths["projects"].Value + "\\" + archiveNames[elem.Value].ToString() + ".7z", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        archiveContent = new byte[fs1.Length];
                        fs1.Read(archiveContent, 0, (int)fs1.Length);
                    }

                    byte[] archiveContentEncrypted = X509Crypto.EncryptMsg(archiveContent, recipientCert);
                    fs.Write(archiveContentEncrypted, 0, archiveContentEncrypted.Length);

                    fs.Write(sysGuid.ToByteArray(), 0, 16);
                    byte[] lenArray = null;
                    ConvertIntToArray(archiveContentEncrypted.Length, out lenArray);
                    fs.Write(lenArray, 0, 4);
                }
            }
        }
    }
}
