using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetCoreApis.Tools.Qa
{
    public class TestJsonWebTokenTools
    {
        private readonly JsonWebTokenTools _jsonWebTokenTools = null;
        private readonly CertificateTools _certificateTools = null;
        private readonly X509Certificate2 _fullCertificate = null;
        private readonly X509Certificate2 _publicCertificate = null;
        readonly Dictionary<string, object> _payload = null;

        public TestJsonWebTokenTools()
        {
            ILogger logger = Mock.Of<ILogger>();
            _jsonWebTokenTools = new JsonWebTokenTools(logger);
            Assert.NotNull(_jsonWebTokenTools);
            _certificateTools = new CertificateTools(logger);
            Assert.NotNull(_certificateTools);
            DateTime tenYearsFromNow = DateTime.UtcNow.AddYears(10);
            _fullCertificate = _certificateTools.CreateSignedPrivateCertificate("ten_year_certificate", "ten_year_certificate_password", tenYearsFromNow);
            Assert.NotNull(_fullCertificate);
            _publicCertificate = _certificateTools.GetPublicCertificate(_fullCertificate);
            Assert.NotNull(_publicCertificate);
            _payload = new Dictionary<string, object>();
            _payload.Add("string", "value");
            _payload.Add("integer", 10);
            _payload.Add("float", 99.99);
        }

        private bool ComparePayload(Dictionary<string, object> expected, Dictionary<string, object> actual)
        {
            if (actual is null || expected is null)
                return false;
            if (expected.Count != actual.Count)
                return false;
            foreach (KeyValuePair<string, object> item in expected)
            {
                if (!expected.ContainsKey(item.Key))
                    return false;
                if (expected[item.Key] != item.Value)
                    return false;
            }
            return true;
        }

        [Fact]
        public void AsymetricToken_ToWithFullCertificate_FromWithFullCertificate()
        {
            string payloadJWT = _jsonWebTokenTools.ToAsymetricToken(_fullCertificate, _payload);
            Assert.NotEmpty(payloadJWT);
            Dictionary<string, object> restoredPayload = _jsonWebTokenTools.FromAsymetricToken(_fullCertificate, payloadJWT);
            Assert.True(ComparePayload(_payload, restoredPayload));
        }

        [Fact]
        public void AsymetricToken_ToWithFullCertificate_FromWithPublicCertificate()
        {
            string payloadJWT = _jsonWebTokenTools.ToAsymetricToken(_fullCertificate, _payload);
            Assert.NotEmpty(payloadJWT);
            Dictionary<string, object> restoredPayload = _jsonWebTokenTools.FromAsymetricToken(_publicCertificate, payloadJWT);
            Assert.True(ComparePayload(_payload, restoredPayload));
        }

        [Fact]
        public void AsymetricToken_ToWithPublicCertificate_FromWithPublicCertificate()
        {
            string payloadJWT = _jsonWebTokenTools.ToAsymetricToken(_publicCertificate, _payload);
            Assert.NotEmpty(payloadJWT);
            Dictionary<string, object> restoredPayload = _jsonWebTokenTools.FromAsymetricToken(_publicCertificate, payloadJWT);
            Assert.False(ComparePayload(_payload, restoredPayload));
        }

        [Fact]
        public void AsymetricToken_ToWithPublicCertificate_FromWithFullCertificate()
        {
            string payloadJWT = _jsonWebTokenTools.ToAsymetricToken(_publicCertificate, _payload);
            Assert.NotEmpty(payloadJWT);
            Dictionary<string, object> restoredPayload = _jsonWebTokenTools.FromAsymetricToken(_fullCertificate, payloadJWT);
            Assert.True(ComparePayload(_payload, restoredPayload));
        }
    }
}
