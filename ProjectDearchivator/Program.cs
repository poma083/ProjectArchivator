using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using ProjectArchivator;

namespace ConsoleApplication1
{
    class Program
    {
        public static void ConvertIntToArray(int x, out byte[] ar)
        {
            ar = new byte[4];
            ar[3] = Convert.ToByte(x & 0xFF);
            ar[2] = Convert.ToByte((x >> 8) & 0xFF);
            ar[1] = Convert.ToByte((x >> 16) & 0xFF);
            ar[0] = Convert.ToByte((x >> 24) & 0xFF);
        }
        public static bool ConvertArrayToInt(byte[] ar, int offset, ref int x)
        {
            if (ar.Length < offset + 4)
            {
                return false;
            }
            x = x & 0x00000000;
            x = x | Convert.ToInt32(ar[offset + 3]);
            x = x | (Convert.ToInt32(ar[offset + 2]) << 8);
            x = x | (Convert.ToInt32(ar[offset + 1]) << 16);
            x = x | (Convert.ToInt32(ar[offset + 0]) << 24);
            return true;
        }
        static void Main(string[] args)
        {
            ProjectArchivatorConfig cfg = ProjectArchivatorConfig.GetConfig();

            // Читаем сертификат из файла.
            byte[] rawData = File.ReadAllBytes(cfg.Enveroument.Paths["cert"].Value + "\\" + cfg.RecepientCert);
            X509Certificate2 certToFind = new X509Certificate2();
            certToFind.Import(rawData);

            // Открываем store "My"
            X509Store store = new X509Store("My", StoreLocation.CurrentUser);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2 ret = null;
            int count = 0;
            foreach (X509Certificate2 cer in store.Certificates)
            {
                if (cer.Equals(certToFind))
                {
                    count++;
                    ret = cer;
                }
            }

            // Проверяем, что нашли ровно один сертификат.
            if (count == 0)
            {
                Console.WriteLine("Сертификат не найден.");
                return;
            }
            // Если сертификат найден, то он только один

            // Получаем секретный ключ соответствующий данному сертификату.
            //AsymmetricAlgorithm asym = ret.PrivateKey;
            if (!ret.HasPrivateKey)
            {
                Console.WriteLine("Нет секретного ключа соответствующего искомому сертификату.");
                return;
            }

            byte[] fileContent = null;
            using (FileStream fs = new FileStream(cfg.Enveroument.Paths["projects"].Value + "\\" + cfg.Source, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileContent = new byte[fs.Length];
                fs.Read(fileContent, 0, (int)fs.Length);
            }
            int i = 0;
            byte[] guidByte = new byte[16];
            Guid sysGuid = new Guid(cfg.SystemGuid);
            Guid tmpGuid = new Guid(cfg.SystemGuid);
            byte[] lenByte = new byte[4];
            int archiveLength = 0;
            int position = fileContent.Length;
            List<string> archFiles = new List<string>();
            while (true)
            {
                i++;
                Array.Copy(fileContent, position - lenByte.Length, lenByte, 0, lenByte.Length);
                position -= lenByte.Length;
                Array.Copy(fileContent, position - guidByte.Length, guidByte, 0, guidByte.Length);
                position -= guidByte.Length;
                ConvertArrayToInt(lenByte, 0, ref archiveLength);
                tmpGuid = new Guid(guidByte);
                if(!tmpGuid.Equals(sysGuid))
                {
                    break;
                }
                byte[] archiveContentEncrypted = new byte[archiveLength];
                Array.Copy(fileContent, position - archiveLength, archiveContentEncrypted, 0, archiveLength);
                position -= archiveLength;
                byte[] archiveContent = X509Crypto.DecryptMsg(archiveContentEncrypted, new X509Certificate2Collection(ret));
                Guid tmp = Guid.NewGuid();
                archFiles.Add(tmp.ToString());
                using (FileStream fs = new FileStream(cfg.Enveroument.Paths["src"].Value + "\\" + tmp.ToString() + ".7z", FileMode.Create))
                {
                    fs.Write(archiveContent, 0, (int)archiveContent.Length);
                }
            }

            Directory.SetCurrentDirectory(cfg.Enveroument.Paths["src"].Value);
            foreach(string archive in archFiles){
                _7ZipNativeContainer.SevenZipExtractArchive(0, 0, cfg.Enveroument.Paths["src"].Value + "\\" + archive + ".7z", true, true, false, cfg.Source, "myMPass", "");
                File.Delete(cfg.Enveroument.Paths["src"].Value + "\\" + archive + ".7z");
            }
        }
    }
}
