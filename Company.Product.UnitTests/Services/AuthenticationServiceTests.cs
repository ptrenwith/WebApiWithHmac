using Company.Product.AuthenticationService;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Text;

namespace Company.Product.UnitTests.Services
{
    public class AuthenticationServiceTests
    {
        private ServiceProvider _container;
        private HMACAuthentication _authService;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var builder = new ServiceCollection();

            builder.AddScoped<HMACAuthentication>();

            _container = builder.BuildServiceProvider();

            _authService = _container.GetRequiredService<HMACAuthentication>();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _container?.Dispose();
        }

        [Test]
        public void HMACSignatureAndReplayTest()
        {
            var method = "POST";
            var uri = "https://localhost/v1/calculations/dams/capacities";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString();
            var content = "absd1234";

            var signature = HMACAuthentication.ComputeHMACSignature(method, uri, timestamp, nonce, content);

            bool valid = HMACAuthentication.ValidateHMACSignature(method, uri, HMACAuthentication.AppId, timestamp, nonce, content, signature);
            valid.Should().BeTrue();

            valid = HMACAuthentication.ValidateHMACSignature(method, uri, HMACAuthentication.AppId, timestamp, nonce, content, signature);
            valid.Should().BeFalse();
        }

        [Test]
        public void HMACSignatureExpirationTest()
        {
            var method = "POST";
            var uri = "https://localhost/v1/calculations/dams/capacities";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString();
            var content = "absd1234";

            var signature = HMACAuthentication.ComputeHMACSignature(method, uri, timestamp, nonce, content);

            timestamp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString();
            var valid = HMACAuthentication.ValidateHMACSignature(method, uri, HMACAuthentication.AppId, timestamp, nonce, content, signature);
            valid.Should().BeFalse();
        }

        [Test]
        public void Sha256Test()
        {
            var hash = HMACAuthentication.Sha256(Encoding.ASCII.GetBytes(""));
            hash.Should().Be("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        }
    }
}
