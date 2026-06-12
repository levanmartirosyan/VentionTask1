using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VentionTask1.Settings;

namespace VentionTask1.Controllers
{
    [Route("api/ApplicationSettings")]
    [ApiController]
    public class ApplicationSettingsController : ControllerBase
    {
        private readonly ApplicationSettings _settings;

        public ApplicationSettingsController(IOptions<ApplicationSettings> options)
        {
            _settings = options.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_settings);
        }
    }
}
