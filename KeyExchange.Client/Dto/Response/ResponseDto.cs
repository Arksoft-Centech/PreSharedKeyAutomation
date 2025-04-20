using KeyExchange.Client.Enumerations;

namespace KeyExchange.Client.Dto.Response
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class ResponseDto
    {
        public bool IsOk { get; set; }
        public ResponseTypeEnum Status { get; set; }
        public string Message { get; set; }
    }
}
