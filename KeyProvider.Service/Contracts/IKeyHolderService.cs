using KeyProvider.Shared.Dto.Request;
using KeyProvider.Shared.Dto.Response;

namespace KeyProvider.Service.Contracts
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public interface IKeyHolderService
    {
        string GetPublicKey();
        KeyModelDto? GetClientKey(Guid clientGuid);
        KeyModelDto CreateClientKey(RequestPreSharedKeyDto clientKey);
    }
}
