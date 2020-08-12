using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreApis.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNetCoreApis.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShellController : ControllerBase
    {
        private readonly ILogger _logger = null;
        private ICertificateTools _certificateTools = null;
        public ShellController(ILogger<ShellController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute a shell command
        /// </summary>
        /// <param name="command">Command that needs to be executed</param>
        /// <param name="parameters">Parameters for the command</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("Command")]
        public async Task<FileStreamResult> ExecuteCommand(string command = "", string parameters = "")
        {
            await Task.Delay(0);
            try
            {
                var stream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
                return new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = "test.txt"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                //return this.StatusCode(StatusCodes.Status500InternalServerError);
                return null;
            }
        }
        
        [HttpPost("GetFile")]
        public FileResult getFileById(string filePath)
        {
            return PhysicalFile($"{filePath}", "application/octet-stream", enableRangeProcessing: true);
        }
    }
}
