using KeyProvider.Shared.Dto.Request;
using KeyProvider.Shared.Dto.Response;

namespace KeyProvider.Service.Contracts
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public interface IKeyProviderService
    {
        KeyModelDto GetServerPublicKey();
        KeyModelDto CreateClientInfo(RequestPreSharedKeyDto clientKey);
        KeyModelDto? GetPreSharedKey(Guid clientGuid);
    }
}
