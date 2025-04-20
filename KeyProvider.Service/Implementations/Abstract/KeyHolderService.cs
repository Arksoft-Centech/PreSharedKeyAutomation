using KeyProvider.Service.Contracts;
using KeyProvider.Service.Implementations.Base;
using KeyProvider.Shared.Dto.Request;
using KeyProvider.Shared.Dto.Response;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace KeyProvider.Service.Implementations.Abstract
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public sealed class KeyHolderService : ServiceBase, IKeyHolderService
    {
        private readonly ConcurrentDictionary<Guid, KeyModelDto> preSharedKeys;
        private readonly RSA serverRsa;

        public KeyHolderService()
        {
            preSharedKeys = new ConcurrentDictionary<Guid, KeyModelDto>();
            serverRsa = RSA.Create(2048);
        }

        public KeyModelDto CreateClientKey(RequestPreSharedKeyDto clientKey)
        {
            if (clientKey == null || clientKey.ClientPublicKeyCrypted == null || clientKey.ClientPublicKeyCrypted.Count <= 0) return new KeyModelDto();

            var clientPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(DecryptChunks(clientKey.ClientPublicKeyCrypted)));

            if (string.IsNullOrWhiteSpace(clientPublicKeyXml))
            {
                return new KeyModelDto();
            }

            var preSharedKey = GenerateAesKey();
            var ivPaddingKey = GenerateIvPaddingKey();
            var signatureKey = GenerateHmacKey();

            var (preSharedKeyCrypted, ivPaddingKeyCrypted, signatureKeyCrypted) = EncryptWithClientPublicKey(clientPublicKeyXml, preSharedKey, ivPaddingKey, signatureKey);

            var kModel = new KeyModelDto
            {
                ClientGuid = Guid.NewGuid(),
                PreSharedKey = preSharedKeyCrypted,
                IvPaddingKey = ivPaddingKeyCrypted,
                SignatureKey = signatureKeyCrypted
            };

            preSharedKeys.TryAdd(kModel.ClientGuid, new KeyModelDto
            {
                ClientGuid = Guid.NewGuid(),
                PreSharedKey = preSharedKey,
                IvPaddingKey = ivPaddingKey,
                SignatureKey = signatureKey
            });

            return kModel;
        }

        public KeyModelDto? GetClientKey(Guid clientGuid)
        {
            return preSharedKeys.TryGetValue(clientGuid, out var keyModelDto) ? keyModelDto : null;
        }

        public string GetPublicKey()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serverRsa.ToXmlString(false)));
        }

        private static string GenerateAesKey()
        {
            var key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        public static string GenerateHmacKey()
        {
            var key = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        public static string GenerateIvPaddingKey()
        {
            var key = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        private static (string, string, string) EncryptWithClientPublicKey(string clientPublicKeyXml, string preSharedKey, string ivPaddingKey, string signatureKey)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(clientPublicKeyXml);

            var preSharedKeyCrypted = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(preSharedKey), RSAEncryptionPadding.Pkcs1));

            var ivPaddingKeyCrypted = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(ivPaddingKey), RSAEncryptionPadding.Pkcs1));

            var signatureKeyCrypted = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(signatureKey), RSAEncryptionPadding.Pkcs1));

            return (preSharedKeyCrypted, ivPaddingKeyCrypted, signatureKeyCrypted);
        }

        private string DecryptChunks(List<string> encryptedChunks)
        {
            var allBytes = new List<byte>();

            foreach (var base64Chunk in encryptedChunks)
            {
                byte[] encryptedChunk = Convert.FromBase64String(base64Chunk);
                byte[] decryptedChunk = serverRsa.Decrypt(encryptedChunk, RSAEncryptionPadding.Pkcs1);
                allBytes.AddRange(decryptedChunk);
            }

            return Encoding.UTF8.GetString([.. allBytes]);
        }
    }
}
