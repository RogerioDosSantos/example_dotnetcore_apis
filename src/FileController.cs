using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNetCoreApis.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ILogger _logger = null;
        public FileController(ILogger<FileController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Download an in memory file
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("inMemoryFile")]
        public async Task<FileStreamResult> GetInMemoryFile()
        {
            await Task.Delay(0);
            try
            {
                MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
                return new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = "test.txt"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        [HttpGet("diskFile")]
        public FileResult getFileById(string filePath)
        {
            return PhysicalFile($"{filePath}", "application/octet-stream", enableRangeProcessing: true);
        }
    }
}
