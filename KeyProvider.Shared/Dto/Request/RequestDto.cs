using System.ComponentModel.DataAnnotations;

namespace KeyProvider.Shared.Dto.Request
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class RequestDto
    {
        [Required]
        public string? ClientMessage { get; set; }
    }
}
