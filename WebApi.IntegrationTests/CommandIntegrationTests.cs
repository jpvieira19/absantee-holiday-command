using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.DTO;
using DataModel.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebApi.IntegrationTests.Helpers;
using Xunit;

namespace WebApi.IntegrationTests.Tests
{
    public class CommandIntegrationTests : IClassFixture<IntegrationTestsWebApplicationFactory<Program>>
    {
        private readonly IntegrationTestsWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CommandIntegrationTests(IntegrationTestsWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task PostHoliday_ReturnsAccepted_WhenValidHoliday()
        {

            // Arrange
            var client = _factory.CreateClient();
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AbsanteeContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var holidayPeriodDTO = new HolidayPeriodDTO(DateOnly.FromDateTime(DateTime.Now),DateOnly.FromDateTime(DateTime.Now.AddDays(10)));
            var holidayDTO = new
            {
                _colabId = 1,
                _holidayPeriod = holidayPeriodDTO 
            };

            var content = new StringContent(JsonConvert.SerializeObject(holidayDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/Holiday", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var returnedDto = JsonConvert.DeserializeObject<HolidayDTO>(responseString);

            Assert.NotNull(returnedDto);
            Assert.Equal(1,returnedDto._colabId);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Now),returnedDto._holidayPeriod.StartDate);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Now.AddDays(10)),returnedDto._holidayPeriod.EndDate);
        }

        [Fact]
        public async Task PostHoliday_ReturnsBadRequest_WhenInvalidHoliday()
        {
            // Arrange
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AbsanteeContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var invalidHolidayPeriodDTO = new HolidayPeriodDTO(DateOnly.FromDateTime(DateTime.Now),DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));
            var invalidHolidayDTO = new
            {
                _colabId = 1,
                _holidayPeriod = invalidHolidayPeriodDTO 
            };

            var content = new StringContent(JsonConvert.SerializeObject(invalidHolidayDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/Holiday", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostHoliday_ReturnsBadRequest_WhenAlreadyHasHolidayInThatPeriod()
        {
            // Arrange
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AbsanteeContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var invalidHolidayPeriodDTO = new HolidayPeriodDTO(DateOnly.FromDateTime(DateTime.Now),DateOnly.FromDateTime(DateTime.Now.AddDays(9)));
            var invalidHolidayDTO = new
            {
                _colabId = 5,
                _holidayPeriod = invalidHolidayPeriodDTO
            };

            var content = new StringContent(JsonConvert.SerializeObject(invalidHolidayDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/Holiday", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
        
    }
}
