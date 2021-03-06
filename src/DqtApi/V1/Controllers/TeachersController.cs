using System.Threading.Tasks;
using DqtApi.Logging;
using DqtApi.V1.Requests;
using DqtApi.V1.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V1.Controllers
{
    [ApiController]
    [Route("teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeachersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{trn}")]
        [SwaggerOperation(
            summary: "Teacher",
            description: "Get an individual teacher by their TRN")]
        [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [RedactQueryParam("birthdate")]
        public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)
        {
            var response = await _mediator.Send(request);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
