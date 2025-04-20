using KeyProvider.Service.Contracts;
using KeyProvider.Service.Implementations.Base;
using KeyProvider.Shared.Dto.Response;
using KeyProvider.Shared.Helpers.Abstract;

namespace KeyProvider.Service.Implementations.Abstract
{
    public sealed class DataService(IKeyProviderService keyProviderService) : ServiceBase, IDataService
    {
        public ResponseDto GetServerAnswer(string? clientMessage, Guid clientGuid)
        {
            if (string.IsNullOrWhiteSpace(clientMessage)) return ResponseDto.GenerateError("Client message empty");

            var clientKeys = keyProviderService.GetPreSharedKey(clientGuid);

            if (clientKeys == null) return ResponseDto.GenerateError();

            var decryptedMessage = EncryptionHelper.GetInstance.Decrypt(clientMessage, clientKeys.PreSharedKey, clientKeys.IvPaddingKey);

            if (string.IsNullOrWhiteSpace(decryptedMessage)) return ResponseDto.GenerateError();

            var serverMessageCrypted = EncryptionHelper.GetInstance.Encrypt($"Hello from server! Client sent: {decryptedMessage}", clientKeys.PreSharedKey, clientKeys.IvPaddingKey);

            return ResponseDto.GenerateSuccess(serverMessageCrypted);
        }
    }
}
