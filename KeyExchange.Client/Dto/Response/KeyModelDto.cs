using System;

namespace KeyProvider.Shared.Dto.Response
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class KeyModelDto
    {
        public string PublicKey { get; set; }
        public string PreSharedKey { get; set; }
        public string IvPaddingKey { get; set; }
        public string SignatureKey { get; set; }
        public Guid ClientGuid { get; set; }
    }
}
