using HealthManager.Models;
using HealthManager.Models.DTO;
using HealthManager.Services.Authentication;
using HealthManagerIntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerIntegrationTests.ControllerTests.AuthTests
{
    [Collection("IntegrationTests")]
    public class RegisterTests : IClassFixture<CustomWebApplicationEnv>
    {
        private HttpClient _httpClient;
        private readonly HealthManagerContext _dbContext;
        private readonly IJWTService _tokenService;

        public RegisterTests(CustomWebApplicationEnv custom)
        {
            _httpClient = custom.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var scope = custom.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<HealthManagerContext>();
            _tokenService = scope.ServiceProvider.GetRequiredService<IJWTService>();
        }

        [Fact]
        public async Task CheckIfPatientRegisterCompletesSuccessfully() 
        {
            //Arrange

            PatientDTO testPatient = new PatientDTO
            {
                Name = "Rosalia",
                Surname = "Benitez",
                Birthdate = new DateOnly(1999, 03, 30),
                Dni = 1234567,
                PhoneNumber = 123456789,
                Email = "Rosalia@gmail.com",
                Password = "Benitez1234!!",
                ConfirmPassword = "Benitez1234!!",
                Gender = "F",
                Sex = "F",
            };

            
            var content = AuxMethods.ConvertClassObjectToFormUrlEncoded(testPatient);

            //Act

            var response = await _httpClient.PostAsync("/Authorize/Register", content);

            //Assert

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        }

        [Fact]
        public async Task CheckThatRegisterFailsIfEmailIsDuplicated()
        {
            //Arrange

            PatientDTO testPatient = new PatientDTO
            {
                Name = "Rosalia",
                Surname = "Benitez",
                Birthdate = new DateOnly(1999, 03, 30),
                Dni = 1234567,
                PhoneNumber = 123456789,
                Email = "aliciarodriguez@gmail.com",
                Password = "Benitez1234!!",
                ConfirmPassword = "Benitez1234!!",
                Gender = "F",
                Sex = "F",
            };

            var formContent = AuxMethods.ConvertClassObjectToFormUrlEncoded(testPatient);

            //Act

            var response = await _httpClient.PostAsync("/Authorize/Register", formContent);
            var content = await response.Content.ReadAsStringAsync();

            //Assert

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("* The provided email is already in use.", content);
        }

        [Fact]
        public async Task RegisterFailsIfDataIsIncomplete()
        {
            //Arrange 

            PatientDTO testPatient = new PatientDTO
            {
                Name = "Rosalia",
                Surname = null,
                Birthdate = new DateOnly(1999, 03, 30),
                Dni = 1234567,
                PhoneNumber = 123456789,
                Email = null,
                Password = "Benitez1234!!",
                ConfirmPassword = null,
                Gender = "F",
                Sex = "F",
            };

            var formContent = AuxMethods.ConvertClassObjectToFormUrlEncoded(testPatient);

            //Act

            var response = await _httpClient.PostAsync("/Authorize/Register", formContent);
            var content = await response.Content.ReadAsStringAsync();

            //Assert

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("* There are missing or invalid fields on the form.", content);
        }
    }
}
