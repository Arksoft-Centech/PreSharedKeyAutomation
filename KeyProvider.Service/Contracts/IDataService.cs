using KeyProvider.Shared.Dto.Response;

namespace KeyProvider.Service.Contracts
{
    public interface IDataService
    {
        ResponseDto GetServerAnswer(string? clientMessage, Guid clientGuid);
    }
}
