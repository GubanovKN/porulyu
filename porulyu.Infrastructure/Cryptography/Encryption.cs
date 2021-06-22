using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace porulyu.Infrastructure.Cryptography
{
    public class Encryption
    {
        private static readonly String Salt = "hwvNQ9&%";
        private static readonly String Vector = "a8doSuDitOz1hZe#";
        private static readonly Int32 PasswordIter = 2;
        private static readonly Int32 KeySize = 256;
        private static readonly String Key = "m5ip&u432%7";

        public String EncryptRSA(String InputPassword)
        {
            try
            {
                byte[] VectorB = Encoding.ASCII.GetBytes(Vector);
                byte[] SaltB = Encoding.ASCII.GetBytes(Salt);
                byte[] InputPasswordB = Encoding.UTF8.GetBytes(InputPassword);

                PasswordDeriveBytes DerivePassword = new PasswordDeriveBytes(Key, SaltB, "SHA1", PasswordIter);
                byte[] KeyBytes = DerivePassword.GetBytes(KeySize / 8);

                RijndaelManaged SymmKey = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                };

                byte[] EncryptrTextBytes = null;

                using (ICryptoTransform Encryptor = SymmKey.CreateEncryptor(KeyBytes, VectorB))
                {
                    using (MemoryStream MemoryStream = new MemoryStream())
                    {
                        using (CryptoStream CryptoStream = new CryptoStream(MemoryStream, Encryptor, CryptoStreamMode.Write))
                        {
                            CryptoStream.Write(InputPasswordB, 0, InputPasswordB.Length);
                            CryptoStream.FlushFinalBlock();
                            EncryptrTextBytes = MemoryStream.ToArray();
                            MemoryStream.Close();
                            CryptoStream.Close();
                        }
                    }
                }

                SymmKey.Clear();

                return Convert.ToBase64String(EncryptrTextBytes);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }

        public String DecryptRSA(String OutputPassword)
        {
            try
            {
                byte[] VectorB = Encoding.ASCII.GetBytes(Vector);
                byte[] SaltB = Encoding.ASCII.GetBytes(Salt);
                byte[] OutputPasswordB = Convert.FromBase64String(OutputPassword);

                PasswordDeriveBytes DerivePassword = new PasswordDeriveBytes(Key, SaltB, "SHA1", PasswordIter);
                byte[] KeyBytes = DerivePassword.GetBytes(KeySize / 8);

                RijndaelManaged SymmKey = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                };

                byte[] InitialTextBytes = new byte[OutputPasswordB.Length];

                Int32 ByteCount = 0;

                using (ICryptoTransform Decryptor = SymmKey.CreateDecryptor(KeyBytes, VectorB))
                {
                    using (MemoryStream MemoryStream = new MemoryStream(OutputPasswordB))
                    {
                        using (CryptoStream CryptoStream = new CryptoStream(MemoryStream, Decryptor, CryptoStreamMode.Read))
                        {
                            ByteCount = CryptoStream.Read(InitialTextBytes, 0, InitialTextBytes.Length);
                            MemoryStream.Close();
                            CryptoStream.Close();
                        }
                    }
                }

                SymmKey.Clear();

                return Encoding.UTF8.GetString(InitialTextBytes, 0, ByteCount);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message, Ex);
            }
        }
    }
}
