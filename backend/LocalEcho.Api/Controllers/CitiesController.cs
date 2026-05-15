using LocalEcho.Aplication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _service;
    public CitiesController(ICityService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetList() 
        => Ok(await _service.GetListAsync());
}