using KeyProvider.Service.Contracts;
using KeyProvider.Service.Implementations.Base;
using KeyProvider.Shared.Dto.Request;
using KeyProvider.Shared.Dto.Response;

namespace KeyProvider.Service.Implementations.Abstract
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public sealed class KeyProviderService(IKeyHolderService keyHolderService) : ServiceBase, IKeyProviderService
    {
        public KeyModelDto CreateClientInfo(RequestPreSharedKeyDto clientKey)
        {
            return keyHolderService.CreateClientKey(clientKey);
        }

        public KeyModelDto? GetPreSharedKey(Guid clientGuid)
        {
            return keyHolderService.GetClientKey(clientGuid);
        }

        public KeyModelDto GetServerPublicKey()
        {
            var kModel = new KeyModelDto
            {
                PublicKey = keyHolderService.GetPublicKey()
            };

            return kModel;
        }
    }
}
