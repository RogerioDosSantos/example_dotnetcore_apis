using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DotNetCoreApis.Tools
{
    public interface ICertificateTools
    {
        X509Certificate2 CreateSignedPrivateCertificate(string certificateName, string certificatePassword, DateTime expiractionDate);
        string ExportToPEM(X509Certificate certificate);
        X509Certificate2 GetCertificateFromFile(string certificatePath, string certificatePassword);
        X509Certificate2 GetPublicCertificate(X509Certificate fullCertificate);
    }

    public class CertificateTools : ICertificateTools
    {
        private readonly ILogger _logger = null;

        public CertificateTools(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create a self signed certificate
        /// </summary>
        /// <param name="certificateName">Name of the certificate</param>
        /// <param name="certificatePassword">Certificate Password</param>
        /// <param name="expiractionDate">Certificate expiration date</param>
        /// <returns></returns>
        public X509Certificate2 CreateSignedPrivateCertificate(string certificateName, string certificatePassword, DateTime expiractionDate)
        {
            SubjectAlternativeNameBuilder nameBuilder = new SubjectAlternativeNameBuilder();
            nameBuilder.AddIpAddress(IPAddress.Loopback);
            nameBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            nameBuilder.AddDnsName("localhost");
            nameBuilder.AddDnsName(Environment.MachineName);
            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certificateName}");
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest certificateRequest = new CertificateRequest(distinguishedName, rsa,
                    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                certificateRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment
                    | X509KeyUsageFlags.KeyEncipherment
                    | X509KeyUsageFlags.DigitalSignature, false));
                certificateRequest.CertificateExtensions.Add(
                   new X509EnhancedKeyUsageExtension(
                       new OidCollection { new Oid("1.9.7.9.1.0.2.7.0") }, false));
                certificateRequest.CertificateExtensions.Add(nameBuilder.Build());
                X509Certificate2 certificate = certificateRequest.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(expiractionDate));
                try
                {
                    certificate.FriendlyName = certificateName;
                }
                catch (Exception)
                {
                    _logger?.LogWarning("Friendly Name is not supported on linux platform. For more information: https://github.com/dotnet/aspnetcore/issues/2038");
                }
                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, certificatePassword), certificatePassword, X509KeyStorageFlags.MachineKeySet);
            }
        }

        /// <summary>
        /// Export a certificate to a PEM format string
        /// </summary>
        /// <param name="certificate">The certificate to export</param>
        /// <returns>A PEM encoded string</returns>
        public string ExportToPEM(X509Certificate certificate)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            return builder.ToString();
        }

        /// <summary>
        /// Export a certificate to a PEM format string
        /// </summary>
        /// <param name="fullCertificate">The full certificate with the private key</param>
        /// <returns>A public certificate</returns>
        public X509Certificate2 GetPublicCertificate(X509Certificate fullCertificate)
        {
            if (fullCertificate is null)
                return null;
            X509Certificate2 publicCertificate = new X509Certificate2(fullCertificate.Export(X509ContentType.Cert));
            return publicCertificate;
        }

        /// <summary>
        /// Get the certificate from a certificate file
        /// </summary>
        /// <param name="certificatePath">Path of the certificate</param>
        /// <param name="certificatePassword">Password of the certificate</param>
        /// <returns>A public certificate</returns>
        public X509Certificate2 GetCertificateFromFile(string certificatePath, string certificatePassword)
        {
            try
            {
                if (string.IsNullOrEmpty(certificatePath) || !File.Exists(certificatePath))
                {
                    _logger?.LogError("Invalid parameters.");
                    return null;
                }
                X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword);
                return certificate;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }
    }
}
