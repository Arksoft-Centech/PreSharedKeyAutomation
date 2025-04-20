using System.Collections.Generic;

namespace KeyProvider.Shared.Dto.Request
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class RequestPreSharedKeyDto
    {
        public List<string> ClientPublicKeyCrypted { get; set; }
    }
}
