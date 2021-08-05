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
    public class JsonWebTokenController : ControllerBase
    {
        private readonly ILogger _logger = null;
        private readonly IJsonWebTokenTools _jsonWebTokenTools = null;
        static private X509Certificate2 _fullCertificate = null;
        static private X509Certificate2 _publicCertificate = null;
        public JsonWebTokenController(ILogger<JsonWebTokenController> logger, IJsonWebTokenTools jsonWebTokenTools, ICertificateTools certificateTools)
        {
            _logger = logger;
            _jsonWebTokenTools = jsonWebTokenTools;
            if (_fullCertificate == null)
            {
                _fullCertificate = certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", DateTime.UtcNow.AddYears(10));
                _publicCertificate = certificateTools.GetPublicCertificate(_fullCertificate);
            }
        }

        /// <summary>
        /// Create a JWT
        /// </summary>
        /// <param name="payloads">Payload List to create the token</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("JWT")]
        public async Task<ActionResult<string>> ToJWT(Dictionary<string, string> payloads)
        {
            await Task.Delay(0);
            try
            {
                Dictionary<string, object> convertedPayloads = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string> payload in payloads)
                {
                    convertedPayloads[payload.Key] = payload.Value;
                }
                string jwt = _jsonWebTokenTools.ToAsymetricToken(_publicCertificate, convertedPayloads);
                return this.Ok(jwt);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Read a JWT
        /// </summary>
        /// <param name="jwt">Payload List to create the token</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("JWT")]
        public async Task<ActionResult<Dictionary<string, string>>> FromJWT(string jwt)
        {
            await Task.Delay(0);
            try
            {
                Dictionary<string, object> payloads = _jsonWebTokenTools.FromAsymetricToken(_fullCertificate, jwt);
                if (payloads == null)
                    return this.StatusCode(StatusCodes.Status204NoContent);
                Dictionary<string, string> response = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> payload in payloads)
                {
                    response[payload.Key] = (string)payload.Value;
                }
                return this.Ok(response);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
