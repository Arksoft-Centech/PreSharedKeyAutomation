using System.ComponentModel.DataAnnotations;

namespace KeyProvider.Shared.Dto.Request
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class RequestPreSharedKeyDto
    {
        [Required]
        public List<string>? ClientPublicKeyCrypted { get; set; }
    }
}
