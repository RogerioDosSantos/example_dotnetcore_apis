using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using dotnetcore_apis.Tools;
using DotNetCoreApis.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IHostingEnvironment _hostingEnvironment = null;

        private string UploadDir {
            get {
                string webRootDir = _hostingEnvironment?.WebRootPath;
                if (string.IsNullOrEmpty(webRootDir))
                {
                    _logger.LogWarning("Could not get web base directory. Using content root path");
                    webRootDir = _hostingEnvironment?.ContentRootPath;
                    if (string.IsNullOrEmpty(webRootDir))
                    {
                        _logger.LogWarning("Could not get web base directory. Using executable path");
                        webRootDir = AppDomain.CurrentDomain.BaseDirectory;
                    }
                }
                return Path.Combine(webRootDir, "uploads");
            }
        }

        public FileController(ILogger<FileController> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Download an in memory file
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("downloadMemoryFile")]
        public async Task<FileStreamResult> DownloadMemoryFile()
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

        /// <summary>
        /// Get a file from the disk
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpGet("downloadDiskFile")]
        public FileResult DownloadDiskFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));
            return PhysicalFile($"{filePath}", "application/octet-stream", enableRangeProcessing: true);
        }

        /// <summary>
        /// Upload small files and save it into the disk (Up to 30MB)
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        /// <response code="500">Internal Error</response>
        /// <response code="413">File too Large</response>
        /// <response code="400">Did not receive any file</response>

        [HttpPost("uploadSmallFiles")]
        public ActionResult<List<FileUploadResponseModel>> UploadSmallFiles([FromForm] IFormFileCollection files)
        {
            List<FileUploadResponseModel> savedProperties = null;
            if (FileTools.SaveFilesToDisk(UploadDir, files, out savedProperties))
                return Ok(savedProperties);

            return Ok(savedProperties);
        }

        /// <summary>
        /// Upload large files and save it into the disk (More than 30MB)
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        /// <response code="500">Internal Error</response>
        /// <response code="413">File too Large</response>
        /// <response code="400">Did not receive any file</response>
        [HttpPost("uploadLargeFiles")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        public ActionResult<List<FileUploadResponseModel>> UploadLargeFiles([FromForm] IFormFileCollection files)
        {
            List<FileUploadResponseModel> savedProperties = null;
            if (FileTools.SaveFilesToDisk(UploadDir, files, out savedProperties))
                return Ok(savedProperties);

            return Ok(savedProperties);
        }


    }
}
