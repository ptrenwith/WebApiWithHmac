using Company.Product.Services.Interfaces;
using Company.Product.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Company.Product.UnitTests.Services
{
    public class CapacityServiceTests
    {
        private ServiceProvider _container;
        private ICapacityService _capacityService;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var builder = new ServiceCollection();

            builder.AddScoped<ICapacityService, CapacityService>();

            _container = builder.BuildServiceProvider();

            _capacityService = _container.GetRequiredService<ICapacityService>();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _container?.Dispose();
        }

        [Test]
        public void CalculateCapacityFromElevationExpect5Test()
        {
            int capacity = _capacityService.CalculateDamCapacityFromElevation(new int[] { 2, 3, 2, 1, 3, 1, 1, 2, 1 });
            capacity.Should().Be(5);
        }

        [Test]
        public void CalculateCapacityFromElevationExpect12Test()
        {
            int capacity = _capacityService.CalculateDamCapacityFromElevation(new int[] { 3, 1, 1, 2, 1, 4, 3, 1, 2, 1, 3 });
            capacity.Should().Be(12);
        }

        [Test]
        public void CalculateCapacityFromElevationExpect3Test()
        {
            int capacity = _capacityService.CalculateDamCapacityFromElevation(new int[] { 3, 1, 2, 3, 1 });
            capacity.Should().Be(3);
        }

        [Test]
        public void CalculateCapacityFromElevationExpect0Test()
        {
            int capacity = _capacityService.CalculateDamCapacityFromElevation(new int[] { 1, 2, 3, 3, 2, 1 });
            capacity.Should().Be(0);
        }

        [Test]
        public void CalculateCapacityFromElevationExpect16Test()
        {
            int capacity = _capacityService.CalculateDamCapacityFromElevation(new int[] { 3, 2, 1, 0, -1, 0, 1, 2, 3 });
            capacity.Should().Be(16);
        }
    }
}
