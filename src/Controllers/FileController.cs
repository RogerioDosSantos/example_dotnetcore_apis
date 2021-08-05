﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        /// Upload a files and save it into the disk (Up to 30MB)
        /// </summary>
        /// <param name="postedFiles"></param>
        /// <returns></returns>
        [HttpPost("uploadSmallFiles")]
        public ActionResult<List<FileUploadResponseModel>> UploadSmallFiles(List<IFormFile> postedFiles)
        {
            try
            {
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

                string uploadDir = Path.Combine(webRootDir, "uploads");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                List<FileUploadResponseModel> resp = new List<FileUploadResponseModel>();
                foreach (IFormFile postedFile in postedFiles)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);
                    bool fileExist = System.IO.File.Exists(filePath);
                    float fileSize = ((float) postedFile.Length)/(1024 * 1024);
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(fileStream);
                        _logger.LogInformation($"{fileName} - Uploaded with success");
                        resp.Add(new FileUploadResponseModel
                        {
                            file = fileName,
                            uploadPath = filePath,
                            fileReplaced = fileExist,
                            fileSize = fileSize
                        });
                    }
                }
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return Ok(null);
            }

        }
    }
}