using KeyExchange.Client.Dto.Request;
using KeyExchange.Client.Dto.Response;
using KeyExchange.Client.Extensions;
using KeyExchange.Client.Items;
using KeyProvider.Shared.Dto.Request;
using KeyProvider.Shared.Dto.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyExchange.Client
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    internal class Program
    {
        static readonly ManualResetEvent quitEvent = new ManualResetEvent(false);
        static readonly HttpClient httpClient = new HttpClient();
        static readonly RSA clientRsa = RSA.Create(2048);
        static InternalKeyItem preSharedKeys;

        static async Task Main()
        {
            Console.CancelKeyPress += (ss, ee) =>
            {
                ee.Cancel = true;
                quitEvent.Set();
            };

            var handShakeResult = await MakeServerHandshake();

            if (!handShakeResult) return;

            Console.WriteLine("Handshake completed successfully!\n");

            while (!quitEvent.WaitOne(0))
            {
                Console.Write("Send crypto message to server: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                var response = await SendMessageToServerAsync(input);

                if (response == null || response.Status == Enumerations.ResponseTypeEnum.BadCrypto)
                {
                    Console.WriteLine("Handshake corrupted! Trying again...");

                    handShakeResult = await MakeServerHandshake();

                    while (!handShakeResult)
                    {
                        handShakeResult = await MakeServerHandshake();
                    }

                    Console.WriteLine("Handshake completed successfully!\n");

                    continue;
                }

                Console.WriteLine($"Server answer: {JsonConvert.SerializeObject(response)}\n\n");
            }
        }

        static async Task<bool> MakeServerHandshake()
        {
            preSharedKeys = null;
            httpClient.DefaultRequestHeaders.Remove("X-Client-Guid");

            var result = await httpClient.GetAsync("https://localhost:7197/api/key/GetPublicKey");

            if (result == null || !result.IsSuccessStatusCode)
            {
                Console.WriteLine("Server access error");

                return false;
            }

            var responseContent = await result.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                Console.WriteLine("Server response error");

                return false;
            }

            var serverPublicKeyModel = JsonConvert.DeserializeObject<KeyModelDto>(responseContent);

            if (serverPublicKeyModel == null || string.IsNullOrWhiteSpace(serverPublicKeyModel.PublicKey))
            {
                Console.WriteLine("Server public key error");

                return false;
            }

            var serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyModel.PublicKey));

            if (string.IsNullOrWhiteSpace(serverPublicKeyXml))
            {
                Console.WriteLine("Server public key format error");

                return false;
            }

            var clientPublicXml = clientRsa.ToXmlString(false);

            var clientRsaStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicXml));

            var chunkedClientPublic = EncryptPublicKeyInChunks(clientRsaStr, serverPublicKeyXml);

            var keyRequest = new RequestPreSharedKeyDto
            {
                ClientPublicKeyCrypted = chunkedClientPublic
            };

            string json = JsonConvert.SerializeObject(keyRequest);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await httpClient.PostAsync("https://localhost:7197/api/key/CreateClientKey", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request shared key error");

                    return false;
                }

                var keyResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(keyResponse))
                {
                    Console.WriteLine("Shared key format error");

                    return false;
                }

                var keyModel = JsonConvert.DeserializeObject<KeyModelDto>(keyResponse);

                if (keyModel == null || string.IsNullOrWhiteSpace(keyModel.PreSharedKey))
                {
                    Console.WriteLine("Shared key empty");

                    return false;
                }

                var preSharedKey = Encoding.UTF8.GetString(clientRsa.Decrypt(Convert.FromBase64String(keyModel.PreSharedKey), RSAEncryptionPadding.Pkcs1));

                var ivPaddingKey = Encoding.UTF8.GetString(clientRsa.Decrypt(Convert.FromBase64String(keyModel.IvPaddingKey), RSAEncryptionPadding.Pkcs1));

                var signatureKey = Encoding.UTF8.GetString(clientRsa.Decrypt(Convert.FromBase64String(keyModel.SignatureKey), RSAEncryptionPadding.Pkcs1));

                if (string.IsNullOrWhiteSpace(preSharedKey) || string.IsNullOrWhiteSpace(ivPaddingKey) || string.IsNullOrWhiteSpace(signatureKey))
                {
                    Console.WriteLine("Shared key conversion error");

                    return false;
                }

                preSharedKeys = new InternalKeyItem
                {
                    ClientGuid = keyModel.ClientGuid,
                    PreSharedKey = preSharedKey,
                    IvPaddingKey = ivPaddingKey,
                    SignatureKey = signatureKey
                };

                return true;
            }
        }

        static List<string> EncryptPublicKeyInChunks(string data, string serverPublicKeyXml, int rsaKeySizeBits = 2048)
        {
            using (var serverRsa = new RSACryptoServiceProvider(rsaKeySizeBits))
            {
                serverRsa.FromXmlString(serverPublicKeyXml);

                var dataBytes = Encoding.UTF8.GetBytes(data);
                var maxChunkSize = rsaKeySizeBits / 8 - 11;

                var encryptedChunks = new List<string>();

                for (var i = 0; i < dataBytes.Length; i += maxChunkSize)
                {
                    var chunkSize = Math.Min(maxChunkSize, dataBytes.Length - i);
                    var chunk = new byte[chunkSize];
                    Array.Copy(dataBytes, i, chunk, 0, chunkSize);

                    byte[] encryptedChunk = serverRsa.Encrypt(chunk, false);
                    encryptedChunks.Add(Convert.ToBase64String(encryptedChunk));
                }

                return encryptedChunks;
            }
        }

        static async Task<ResponseDto> SendMessageToServerAsync(string input)
        {
            if (!httpClient.DefaultRequestHeaders.Contains("X-Client-Guid"))
            {
                httpClient.DefaultRequestHeaders.Add("X-Client-Guid", preSharedKeys.ClientGuid.ToString());
            }

            var message = new RequestDto
            {
                ClientMessage = Encrypt(input)
            };

            var body = JsonConvert.SerializeObject(message);

            using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
            {
                var response = await httpClient.PostAsync("https://localhost:7197/api/data/GetServerAnswer", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    return null;
                }

                var responseObject = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

                if (responseObject == null || string.IsNullOrWhiteSpace(responseObject.Message)) return null;

                responseObject.Message = Decrypt(responseObject.Message);

                return responseObject;
            }
        }

        static string Encrypt(string plainText)
        {
            var outStr = string.Empty;

            try
            {
                using (var key = new Rfc2898DeriveBytes(preSharedKeys.PreSharedKey, Encoding.ASCII.GetBytes(preSharedKeys.IvPaddingKey), 100000, HashAlgorithmName.SHA256))
                using (var aesAlg = Aes.Create())
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                    using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        outStr = Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch
            {
                Console.WriteLine("Crypto error!");
            }

            return outStr;
        }

        static string Decrypt(string cipherText)
        {
            var plaintext = string.Empty;

            try
            {
                using (var key = new Rfc2898DeriveBytes(preSharedKeys.PreSharedKey, Encoding.ASCII.GetBytes(preSharedKeys.IvPaddingKey), 100000, HashAlgorithmName.SHA256))
                using (var aesAlg = Aes.Create())
                {
                    var bytes = Convert.FromBase64String(cipherText);

                    using (var msDecrypt = new MemoryStream(bytes))
                    {
                        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                        aesAlg.IV = msDecrypt.ReadAsByteArray();

                        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Crypto error!");
            }

            return plaintext;
        }
    }
}
