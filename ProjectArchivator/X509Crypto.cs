using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace ProjectArchivator
{
    public class X509Crypto
    {
        // Подписываем сообщение секретным ключем.
        public static byte[] SignMsg(Byte[] msg, X509Certificate2 signerCert, bool detached)
        {
            // Создаем объект ContentInfo по сообщению.
            // Это необходимо для создания объекта SignedCms.
            ContentInfo contentInfo = new ContentInfo(msg);

            // Создаем объект SignedCms по только что созданному
            // объекту ContentInfo.
            // SubjectIdentifierType установлен по умолчанию в 
            // IssuerAndSerialNumber.
            // Свойство Detached устанавливаем явно в true, таким 
            // образом сообщение будет отделено от подписи.
            SignedCms signedCms = new SignedCms(contentInfo, detached);

            // Определяем подписывающего, объектом CmsSigner.
            CmsSigner cmsSigner = new CmsSigner(signerCert);

            // Подписываем CMS/PKCS #7 сообение.
            Console.Write("Вычисляем подпись сообщения для субъекта " +
                "{0} ... ", signerCert.SubjectName.Name);
            signedCms.ComputeSignature(cmsSigner);
            Console.WriteLine("Успешно.");

            // Кодируем CMS/PKCS #7 подпись сообщения.
            return signedCms.Encode();
        }

        // Проверяем SignedCms сообщение и возвращаем Boolean
        // значение определяющее результат проверки.
        public static bool VerifyMsg(Byte[] msg, byte[] encodedSignature)
        {
            // Создаем объект ContentInfo по сообщению.
            // Это необходимо для создания объекта SignedCms.
            ContentInfo contentInfo = new ContentInfo(msg);

            // Создаем SignedCms для декодирования и проверки.
            SignedCms signedCms = new SignedCms(contentInfo, true);

            // Декодируем подпись
            signedCms.Decode(encodedSignature);

            // Перехватываем криптографические исключения, для 
            // возврата о false значения при некорректности подписи.
            try
            {
                // Проверяем подпись. В данном примере не 
                // проверяется корректность сертификата подписавшего.
                // В рабочем коде, скорее всего потребуется построение
                // и проверка корректности цепочки сертификата.
                Console.Write("Проверка подписи сообщения ... ");
                signedCms.CheckSignature(true);
                Console.WriteLine("Успешно.");
            }
            catch (System.Security.Cryptography.CryptographicException e)
            {
                Console.WriteLine("Функция VerifyMsg возбудила исключение:  {0}",
                    e.Message);
                Console.WriteLine("Проверка PKCS #7 сообщения завершилась " +
                    "неудачно. Возможно сообщене, подпись, или " +
                    "соподписи модифицированы в процессе передачи или хранения. " +
                    "Подписавший или соподписавшие возможно не те " +
                    "за кого себя выдают. Достоверность и/или целостность " +
                    "сообщения не гарантируется. ");
                return false;
            }

            return true;
        }

        // Зашифровываем сообщение, используя открытый ключ 
        // получателя, при помощи класса EnvelopedCms.
        public static byte[] EncryptMsg(Byte[] msg, X509Certificate2 recipientCert)
        {
            // Помещаем сообщение в объект ContentInfo 
            // Это требуется для создания объекта EnvelopedCms.
            ContentInfo contentInfo = new ContentInfo(msg);

            // Создаем объект EnvelopedCms, передавая ему
            // только что созданный объект ContentInfo.
            // Используем идентификацию получателя (SubjectIdentifierType)
            // по умолчанию (IssuerAndSerialNumber).
            // Не устанавливаем алгоритм зашифрования тела сообщения:
            // ContentEncryptionAlgorithm устанавливается в 
            // RSA_DES_EDE3_CBC, несмотря на это, при зашифровании
            // сообщения в адрес получателя с ГОСТ сертификатом,
            // будет использован алгоритм GOST 28147-89.
            EnvelopedCms envelopedCms = new EnvelopedCms(contentInfo);

            // Создаем объект CmsRecipient, который 
            // идентифицирует получателя зашифрованного сообщения.
            CmsRecipient recip1 = new CmsRecipient(
                SubjectIdentifierType.IssuerAndSerialNumber,
                recipientCert);

            Console.Write(
                "Зашифровываем данные для одного получателя " +
                "с именем {0} ...",
                recip1.Certificate.SubjectName.Name);
            // Зашифровываем сообщение.
            envelopedCms.Encrypt(recip1);
            Console.WriteLine("Выполнено.");

            // Закодированное EnvelopedCms сообщение содержит
            // зашифрованный текст сообщения и информацию
            // о каждом получателе данного сообщения.
            return envelopedCms.Encode();
        }

        // Расшифрование закодированного EnvelopedCms сообщения.
        public static Byte[] DecryptMsg(byte[] encodedEnvelopedCms, X509Certificate2Collection certs)
        {
            // Создаем объект для декодирования и расшифрования.
            EnvelopedCms envelopedCms = new EnvelopedCms();

            // Декодируем сообщение.
            envelopedCms.Decode(encodedEnvelopedCms);

            // Выводим количество получателей сообщения
            // (в данном примере должно быть равно 1) и
            // алгоритм зашифрования.
            DisplayEnvelopedCms(envelopedCms, false);

            // Расшифровываем сообщение для единственного 
            // получателя.
            Console.Write("Расшифрование ... ");
            envelopedCms.Decrypt(envelopedCms.RecipientInfos[0], certs);
            Console.WriteLine("Выполнено.");

            // После вызова метода Decrypt в свойстве ContentInfo 
            // содержится расшифрованное сообщение.
            return envelopedCms.ContentInfo.Content;
        }

        // Отображаем свойство ContentInfo объекта EnvelopedCms 
        static private void DisplayEnvelopedCmsContent(String desc, EnvelopedCms envelopedCms)
        {
            Console.WriteLine(desc + " (длина {0}):  ",
                envelopedCms.ContentInfo.Content.Length);
            foreach (byte b in envelopedCms.ContentInfo.Content)
            {
                Console.Write(b.ToString() + " ");
            }
            Console.WriteLine();
        }

        // Отображаем некоторые свойства объекта EnvelopedCms.
        static private void DisplayEnvelopedCms(EnvelopedCms e, Boolean displayContent)
        {
            Console.WriteLine("{0}Закодированное CMS/PKCS #7 Сообщение.{0}" +
                "Информация:", Environment.NewLine);
            Console.WriteLine("\tАлгоритм шифрования сообщения:{0}",
                e.ContentEncryptionAlgorithm.Oid.FriendlyName);
            Console.WriteLine(
                "\tКоличество получателей закодированного CMS/PKCS #7 сообщения:{0}",
                e.RecipientInfos.Count);
            for (int i = 0; i < e.RecipientInfos.Count; i++)
            {
                Console.WriteLine(
                    "\tПолучатель #{0} тип {1}.",
                    i + 1,
                    e.RecipientInfos[i].RecipientIdentifier.Type);
            }
            if (displayContent)
            {
                DisplayEnvelopedCmsContent("Закодированное CMS/PKCS " +
                    "#7 содержимое", e);
            }
            Console.WriteLine();
        }
    }
}
