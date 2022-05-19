using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[ApiController]
[Route("command")]
public sealed class SlackController : ControllerBase
{
    private readonly ISearchContentUseCase _searchContentUseCase;

    public SlackController(ISearchContentUseCase searchContentUseCase)
    {
        _searchContentUseCase = searchContentUseCase ?? throw new ArgumentNullException(nameof(searchContentUseCase));
          
    }


    [HttpPost("/command/search")]
    [Consumes("application/x-www-form-urlencoded")]
    public ActionResult Search([FromForm(Name = "text")] string text, [FromForm(Name = "response_url")]string responseUrl)
    {
        _searchContentUseCase
            .Handle(text, responseUrl);
        
        return Ok();
    }
}