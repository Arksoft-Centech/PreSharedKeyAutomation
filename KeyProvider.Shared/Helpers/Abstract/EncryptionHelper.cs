using KeyProvider.Shared.Extensions;
using KeyProvider.Shared.Helpers.Base;
using System.Security.Cryptography;
using System.Text;

namespace KeyProvider.Shared.Helpers.Abstract
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public sealed class EncryptionHelper : HelperBase<EncryptionHelper>
    {
        public string Encrypt(string plainText, string preSharedKey, string ivPaddingKey)
        {
            try
            {
                using var key = new Rfc2898DeriveBytes(preSharedKey, Encoding.ASCII.GetBytes(ivPaddingKey), 100000, HashAlgorithmName.SHA256);
                using var aesAlg = Aes.Create();

                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using var msEncrypt = new MemoryStream();

                msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch
            { }

            return string.Empty;
        }

        public string Decrypt(string cipherText, string preSharedKey, string ivPaddingKey)
        {
            try
            {
                using var key = new Rfc2898DeriveBytes(preSharedKey, Encoding.ASCII.GetBytes(ivPaddingKey), 100000, HashAlgorithmName.SHA256);
                using var aesAlg = Aes.Create();

                var bytes = Convert.FromBase64String(cipherText);

                using var msDecrypt = new MemoryStream(bytes);

                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = msDecrypt.ReadAsByteArray();

                using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch
            { }

            return string.Empty;
        }
    }
}
