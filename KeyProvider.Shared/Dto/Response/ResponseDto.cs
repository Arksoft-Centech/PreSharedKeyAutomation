using KeyProvider.Shared.Enumerations;

namespace KeyProvider.Shared.Dto.Response
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class ResponseDto
    {
        public bool IsOk { get; set; }
        public ResponseTypeEnum Status { get; set; }
        public string? Message { get; set; }

        public static ResponseDto GenerateSuccess(string? message = null)
        {
            return new ResponseDto { IsOk = true, Status = ResponseTypeEnum.Success, Message = message };
        }

        public static ResponseDto GenerateError(string? message = null)
        {
            return new ResponseDto { Status = ResponseTypeEnum.BadCrypto, Message = message };
        }
    }
}
