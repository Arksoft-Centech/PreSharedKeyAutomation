using KeyProvider.Api.Attributes;
using KeyProvider.Service.Contracts;
using KeyProvider.Shared.Dto.Request;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KeyProvider.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController(IDataService dataService) : ControllerBase
    {
        [HttpPost("GetServerAnswer")]
        [RequireClientGuidHeader]
        public ActionResult GetServerAnswer([FromBody, Required] RequestDto requestDto)
        {
            var clientGuid = (Guid)HttpContext.Items["ClientGuid"]!;

            var serverAnswer = dataService.GetServerAnswer(requestDto.ClientMessage, clientGuid);

            return serverAnswer.IsOk ? Ok(serverAnswer) : BadRequest(serverAnswer);
        }
    }
}
