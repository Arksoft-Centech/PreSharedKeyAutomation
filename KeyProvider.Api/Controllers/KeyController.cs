using KeyProvider.Service.Contracts;
using KeyProvider.Shared.Dto.Request;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KeyProvider.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyController(IKeyProviderService keyProviderService) : ControllerBase
    {
        [HttpGet("GetPublicKey")]
        public ActionResult GetPublicKey()
        {
            return Ok(keyProviderService.GetServerPublicKey());
        }

        [HttpPost("CreateClientKey")]
        public ActionResult CreateClientKey([FromBody, Required] RequestPreSharedKeyDto requestDto)
        {
            return Ok(keyProviderService.CreateClientInfo(requestDto));
        }
    }
}
