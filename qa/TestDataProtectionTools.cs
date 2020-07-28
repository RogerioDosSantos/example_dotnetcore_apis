using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DotNetCoreApis.Tools;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Security.Cryptography;

namespace DotNetCoreApis.Tools.Qa
{
    public class TestDataProtectionTools
    {
        private DataProtectionTools _dataProtectionTools = null;
        private CertificateTools _certificateTools = null;

        public TestDataProtectionTools()
        {
            ILogger logger = Mock.Of<ILogger>();
            _dataProtectionTools = new DataProtectionTools(logger, null);
            _certificateTools = new CertificateTools(logger);
        }

        [Fact]
        public void TestGetDataProtectionProvider_DefaultProvider()
        {
            IDataProtectionProvider dataProtectionProvider = _dataProtectionTools.GetDataProtectionProvider();
            Assert.Null(dataProtectionProvider); // Default provider should be null
        }

        [Fact]
        public void TestGetDataProtectionProvider_NoCertificateInformed()
        {            
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(certificate);
            IDataProtectionProvider dataProtectionProvider = _dataProtectionTools.GetDataProtectionProvider("no_certificate_should_return_null", certificate);
            Assert.NotNull(dataProtectionProvider);
            IDataProtector protector = dataProtectionProvider.CreateProtector("Program.No-DI");

        }

        [Fact]
        public void TestGetDataProtectionProvider_PrivateAndPublicCertificate()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(certificate);
            IDataProtectionProvider dataProtectionProvider = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_PrivateAndPublicCertificate", certificate);
            Assert.NotNull(dataProtectionProvider);
            IDataProtector protector = dataProtectionProvider.CreateProtector("test_data_protection.private_and_public_certificate");
            Assert.NotNull(protector);
            string payload = "payload content";
            string protectedPayload = protector.Protect(payload);
            Assert.NotNull(protectedPayload);
            string unprotectedPayload = protector.Unprotect(protectedPayload);
            Assert.Equal(payload, unprotectedPayload);
        }

        [Fact]
        public void TestGetDataProtectionProvider_TwoProvidersWithSameCertificate()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            X509Certificate2 certificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(certificate);
            IDataProtectionProvider dataProtectionProvider1 = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_01", certificate);
            Assert.NotNull(dataProtectionProvider1);
            IDataProtector protector1 = dataProtectionProvider1.CreateProtector("test_data_protection.protect_provider1.unprotect_provider2");
            Assert.NotNull(protector1);
            string payload = "payload content";
            string protectedPayload = protector1.Protect(payload);
            Assert.NotNull(protectedPayload);
            IDataProtectionProvider dataProtectionProvider2 = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_02", certificate);
            Assert.NotNull(dataProtectionProvider2);
            IDataProtector protector2 = dataProtectionProvider2.CreateProtector("test_data_protection.protect_provider1.unprotect_provider2");
            Assert.NotNull(protector2);
            foreach (string key in Directory.GetFiles(
                _dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_01"), "*.xml"))
            {
                Directory.CreateDirectory(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_02"));
                File.Copy(key, Path.Combine(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_02"),
                    Path.GetFileName(key)), true);
            }
            string unprotectedPayload = protector2.Unprotect(protectedPayload);
            Assert.Equal(payload, unprotectedPayload);
            Directory.Delete(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_01"), true);
            Directory.Delete(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_TwoProvidersWithSameCertificate_02"), true);
        }

        [Fact]
        //[ExpectedException(typeof(ArgumentException), "A userId of null was inappropriately allowed.")]
        public void TestGetDataProtectionProvider_CannotEncryptWithPublicCertificate()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            X509Certificate2 fullCertificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(fullCertificate);
            X509Certificate2 publicCertificate = _certificateTools.GetPublicCertificate(fullCertificate);
            IDataProtectionProvider dataProtectionProvider1 = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_CannotEncryptWithPublicCertificate", publicCertificate);
            Assert.NotNull(dataProtectionProvider1);
            IDataProtector protector1 = dataProtectionProvider1.CreateProtector("test_data_protection.protect_provider1.unprotect_provider2");
            Assert.NotNull(protector1);
            string payload = "payload content";
            Assert.Throws<CryptographicException>(() => protector1.Protect(payload));
            Directory.Delete(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotEncryptWithPublicCertificate"), true);
        }

        [Fact]
        public void TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate()
        {
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            X509Certificate2 fullCertificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(fullCertificate);
            IDataProtectionProvider dataProtectionProvider1 = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_01", fullCertificate);
            Assert.NotNull(dataProtectionProvider1);
            IDataProtector protector1 = dataProtectionProvider1.CreateProtector("test_data_protection.protect_provider1.unprotect_provider2");
            Assert.NotNull(protector1);
            string payload = "payload content";
            string protectedPayload = protector1.Protect(payload);
            Assert.NotNull(protectedPayload);
            X509Certificate2 publicCertificate = _certificateTools.GetPublicCertificate(fullCertificate);
            Assert.NotNull(publicCertificate);
            IDataProtectionProvider dataProtectionProvider2 = _dataProtectionTools.GetDataProtectionProvider("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_02", publicCertificate);
            Assert.NotNull(dataProtectionProvider2);
            IDataProtector protector2 = dataProtectionProvider2.CreateProtector("test_data_protection.protect_provider1.unprotect_provider2");
            Assert.NotNull(protector2);
            foreach (string key in Directory.GetFiles(
                _dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_01"), "*.xml"))
            {
                Directory.CreateDirectory(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_02"));
                File.Copy(key, Path.Combine(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_02"),
                    Path.GetFileName(key)), true);
            }
            Assert.Throws<CryptographicException>(() => protector2.Unprotect(protectedPayload));
            Directory.Delete(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_01"), true);
            Directory.Delete(_dataProtectionTools.GetDataProtectionKeyFolder("TestGetDataProtectionProvider_CannotDecryptWithPublicCertificate_02"), true);
        }
    }
}
