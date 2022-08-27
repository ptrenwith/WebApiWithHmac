using Company.Product.AuthenticationService;
using Company.Product.IntegrationTests.Extentions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Company.Product.IntegrationTests
{
    public class WebApiTests : IDisposable
    {
        private HttpClient _httpClient;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            HMACDelegatingHandler customDelegatingHandler = new HMACDelegatingHandler();
            _httpClient = HttpClientFactory.Create(customDelegatingHandler);
            _httpClient.BaseAddress = new Uri("https://localhost:44372");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Positive Tests

        [Test]
        public async Task CalculateDamElevationTest()
        {
            var request = new StringContent(JsonSerializer.Serialize(new int[] { 2, 1, 2 }), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync($"v1/calculations/dams/capacities", request);
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            var content = await httpResponse.GetContent<int>();
            content.Should().Be(1);
        }

        #endregion

        #region Negative Tests

        [Test]
        public async Task CalculateDamElevationWithNegativeValuesTest()
        {
            var request = new StringContent(JsonSerializer.Serialize(new int[] { 2, -1, 2 }), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync($"v1/calculations/dams/capacities", request);
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }

        #endregion

    }
}
