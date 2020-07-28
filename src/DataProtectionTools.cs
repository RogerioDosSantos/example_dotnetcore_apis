using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNetCoreApis.Tools
{
    public class DataProtectionTools
    {
        private readonly ILogger _logger = null;
        IDataProtectionProvider _dataProtectionProvider = null;

        public DataProtectionTools(ILogger logger, IDataProtectionProvider dataProtectionProvider)
        {
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
        }

        /// <summary>
        /// Get the folder where the data protection provider will add the keys on the system
        /// </summary>
        /// <param name="providerName">An unique name for the provider. Provider with same names allows to separate the encryption on 02 applications</param>
        /// <returns>The data protection provider</returns>
        public string GetDataProtectionKeyFolder(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                return null;
            string keyFolder = Path.Combine(System.Environment.GetEnvironmentVariable("LOCALAPPDATA"), "dotnetcore_apis", "data_protection", "keys", providerName);
            return keyFolder;
        }


        /// <summary>
        /// Get a new Data Protection Provider without dependency injection
        /// </summary>
        /// <param name="providerName">An unique name for the provider. Provider with same names allows to separate the encryption on 02 applications</param>
        /// <param name="certificate">The certificate that will be used to encrypt/decrypt keys</param>
        /// <returns>The data protection provider</returns>
        public IDataProtectionProvider GetDataProtectionProvider(string providerName, X509Certificate2 certificate)
        {
            try
            {
                if (string.IsNullOrEmpty(providerName) || certificate is null)
                    return null;
                string keyFolder = GetDataProtectionKeyFolder(providerName);
                IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(keyFolder), certificate);
                return dataProtectionProvider;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the default data protection provider that was created during the dependency injection.
        /// </summary>
        /// <returns>The data protection provider</returns>
        public IDataProtectionProvider GetDataProtectionProvider()
        {
            return _dataProtectionProvider;
        }

    }
}
