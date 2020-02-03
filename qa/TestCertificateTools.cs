using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace DotNetCoreApis.Tools.Qa
{
    public class TestCertificateTools
    {
        private CertificateTools _certificateTools = null;

        public TestCertificateTools()
        {
            ILogger logger = Mock.Of<ILogger>();
            _certificateTools = new CertificateTools(logger);
        }

        [Fact]
        public void TestBuildSelfSignedPrivateCertificate()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            using (X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate("certificate_name", "certificate_password", tenYearsFromNow))
            {
                Assert.Equal("CN=certificate_name", certificate.Subject);
                Assert.Equal("certificate_name", certificate.FriendlyName);
                Assert.Equal("CN=certificate_name", certificate.Issuer);
                DateTime certificateDateTime;
                Assert.True(DateTime.TryParse(certificate.GetExpirationDateString(), out certificateDateTime));                
                Assert.Equal(tenYearsFromNow.Year, certificateDateTime.Year);
                Assert.Equal(tenYearsFromNow.Month, certificateDateTime.Month);
                Assert.Equal(tenYearsFromNow.Day, certificateDateTime.Day);
            }
        }

        [Fact]
        public void TestExportToPEM()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            using (X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate("certificate_name", "certificate_password", tenYearsFromNow))
            {
                string ret = _certificateTools.ExportToPEM(certificate);
                Assert.StartsWith("-----BEGIN CERTIFICATE-----", ret);
                Assert.EndsWith("-----END CERTIFICATE-----\r\n", ret);
            }
        }        
    }
}
