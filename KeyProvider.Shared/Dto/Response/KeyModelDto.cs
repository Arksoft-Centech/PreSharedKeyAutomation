namespace KeyProvider.Shared.Dto.Response
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class KeyModelDto
    {
        public string PublicKey { get; set; } = null!;
        public string PreSharedKey { get; set; } = null!;
        public string IvPaddingKey { get; set; } = null!;
        public string SignatureKey { get; set; } = null!;
        public Guid ClientGuid { get; set; }
    }
}
