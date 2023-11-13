using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors]
    public class TransactionController : ControllerBase
    {
        
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger)
        {
            _logger = logger;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return Ok(new { });
        }
    }
}