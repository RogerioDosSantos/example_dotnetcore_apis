using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    public class CertificateController : ControllerBase
    {
        private readonly ILogger _logger = null;
        private CertificateTools _certificateTools = null;
        public CertificateController(ILogger<CertificateController> logger)
        {
            _logger = logger;
            _certificateTools = new CertificateTools(_logger);
        }

        /// <summary>
        /// Create a PEM formated certificate.
        /// </summary>
        /// <param name="certificateName">Name of the Certificate</param>
        /// <param name="certificatePassword">Certificate Password</param>
        /// <param name="expirationInYears">Certificate expiration in years</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("Certificate")]
        public async Task<ActionResult<string>> CreateCertificate(string certificateName = "certificate_name", string certificatePassword = "certificate_password", int expirationInYears = 10)
        {
            await Task.Delay(0);
            try
            {
                DateTime expiration = DateTime.UtcNow.AddYears(expirationInYears);
                using (X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate(certificateName, certificatePassword, expiration))
                {
                    string ret = _certificateTools.ExportToPEM(certificate);
                    return this.Ok(ret);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
