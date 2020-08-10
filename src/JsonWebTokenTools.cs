using Jose;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace DotNetCoreApis.Tools
{
    public interface IJsonWebTokenTools
    {
        Dictionary<string, object> FromAsymetricToken(X509Certificate2 signingCertificate, string token);
        string ToAsymetricToken(X509Certificate2 signingCertificate, Dictionary<string, object> payload);
    }

    public class JsonWebTokenTools : IJsonWebTokenTools
    {
        private readonly ILogger _logger = null;

        public JsonWebTokenTools(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Convert a list of objects to a JWT
        /// </summary>
        /// <param name="signingCertificate">The Certificate that will be used to create the JWT</param>
        /// <param name="payload">The JWT Payload that will be signed</param>
        public string ToAsymetricToken(X509Certificate2 signingCertificate, Dictionary<string, object> payload)
        {
            try
            {
                if (signingCertificate is null || payload is null)
                {
                    _logger?.LogError("Invalid parameters.");
                    return "";
                }
                string token = "";
                if (signingCertificate.PrivateKey == null)
                    token = JWT.Encode(payload, signingCertificate.GetRSAPublicKey(), JweAlgorithm.RSA_OAEP, JweEncryption.A256GCM);
                else
                    token = JWT.Encode(payload, signingCertificate.GetRSAPrivateKey(), JwsAlgorithm.RS256);
                return token;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return "";
            }
        }

        /// <summary>
        /// Convert a JWT to a list of objects
        /// </summary>
        /// <param name="signingCertificate">The Certificate that will be used to restore the JWT</param>
        /// <param name="token">The JWT token</param>

        public Dictionary<string, object> FromAsymetricToken(X509Certificate2 signingCertificate, string token)
        {
            try
            {
                if (String.IsNullOrEmpty(token) || signingCertificate is null)
                {
                    _logger?.LogError("Invalid parameters.");
                    return null;
                }
                string jsonPayload = "";
                if (signingCertificate.PrivateKey == null)
                    jsonPayload = Jose.JWT.Decode(token, signingCertificate.GetRSAPublicKey());
                else
                    jsonPayload = JWT.Decode(token, signingCertificate.GetRSAPrivateKey(), JweAlgorithm.RSA_OAEP, JweEncryption.A256GCM);
                Dictionary<string, object> payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonPayload);
                return payload;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }
    }
}
