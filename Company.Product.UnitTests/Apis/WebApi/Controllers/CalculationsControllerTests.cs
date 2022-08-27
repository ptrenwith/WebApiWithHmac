using Company.Product.Services.Interfaces;
using Company.Product.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Company.Product.UnitTests.Apis.WebApi.Controllers
{
    public class CalculationsControllerTests
    {
        private ServiceProvider _container;
        private CalculationsController _calculationsController;
        private Mock<ICapacityService> _mockCapacityService;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var builder = new ServiceCollection();
            _mockCapacityService = new Mock<ICapacityService>();

            builder.AddLogging();
            builder.AddSingleton<ICapacityService>(_mockCapacityService.Object);
            builder.AddScoped<CalculationsController>();

            _container = builder.BuildServiceProvider();

            _calculationsController = _container.GetRequiredService<CalculationsController>();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _container?.Dispose();
        }

        [Test]
        public void CalculateCapacityFromElevationExpect5Test()
        {
            var request = new int[] { 2, 3, 2, 1, 3, 1, 1, 2, 1 };
            var expectedResult = 5;
            _mockCapacityService.Setup(x => x.CalculateDamCapacityFromElevation(request)).Returns(expectedResult);
            IActionResult httpResponse = _calculationsController.CalculateDamCapacity(request);
            httpResponse.Should().BeOfType<OkObjectResult>();
            var objectResult = httpResponse as OkObjectResult;
            objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            var actualResult = (int)objectResult.Value;
            actualResult.Should().Be(expectedResult);
        }
    }
}
