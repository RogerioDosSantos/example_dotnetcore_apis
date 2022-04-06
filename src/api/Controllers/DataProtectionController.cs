using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNetCoreApis.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class DataProtectionController : ControllerBase
    {
        private readonly ILogger _logger = null;
        private readonly IDataProtectionProvider _dataProtectionProvider = null;
        public DataProtectionController(ILogger<DataProtectionController> logger, IDataProtectionProvider dataProtectionProvider)
        {
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
        }

        /// <summary>
        /// Protect Payload.
        /// </summary>
        /// <param name="purpose">Purpose of the data protection</param>
        /// <param name="unprotectedPayload">Payload that will be protected</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("Protect")]
        public async Task<ActionResult<string>> ProtectPayload(string purpose = "test.protection", string unprotectedPayload = "")
        {
            await Task.Delay(0);
            try
            {
                IDataProtector protector = _dataProtectionProvider.CreateProtector(purpose);
                string protectedPayload = protector.Protect(unprotectedPayload);
                return this.Ok(protectedPayload);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Protect Payload.
        /// </summary>
        /// <param name="purpose">Purpose of the data protection</param>
        /// <param name="ProtectedPayload">Payload that will be protected</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("Unprotect")]
        public async Task<ActionResult<string>> UnprotectPayload(string purpose = "test.protection", string ProtectedPayload = "")
        {
            await Task.Delay(0);
            try
            {
                IDataProtector protector = _dataProtectionProvider.CreateProtector(purpose);
                string unprotectedPayload = protector.Unprotect(ProtectedPayload);
                return this.Ok(unprotectedPayload);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
