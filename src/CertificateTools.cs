using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNetCoreApis.Tools
{
    public class CertificateTools
    {
        public CertificateTools()
        {

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
                var certificate = certificateRequest.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(expiractionDate));
                certificate.FriendlyName = certificateName;
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

    }
}
