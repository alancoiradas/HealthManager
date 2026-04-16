using HealthManager.Models;
using HealthManager.Models.DTO;
using HealthManager.Services.Appointments;
using HealthManager.Services.Authentication;
using HealthManagerIntegrationTests;
using HealthManagerIntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerIntegrationTests.ControllerTests.AppointmentTests
{
    [Collection("IntegrationTests")]
    public class AppointmentTests : IClassFixture<CustomWebApplicationEnv>
    {
        private HttpClient _httpClient;
        private readonly HealthManagerContext _dbContext;
        private readonly IJWTService _tokenService;
        private readonly IAppointments _appointmentService;
        public AppointmentTests(CustomWebApplicationEnv custom)
        {
            _httpClient = custom.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var scope = custom.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<HealthManagerContext>();
            _tokenService = scope.ServiceProvider.GetRequiredService<IJWTService>();
            _appointmentService = scope.ServiceProvider.GetRequiredService<IAppointments>();
        }

        [Fact]
        public async Task CheckIfAppointmentReservationsIsSuccessful()
        {
            //Arrange

            Patient patientTest = _dbContext.Patients.Where(x => x.PatientId == 1).FirstOrDefault();
            Doctor doctorTest = _dbContext.Doctors.Where(x => x.DoctorId == 1).FirstOrDefault();

            AppointmentViewModel appointment = new AppointmentViewModel
            {
                PatientId = patientTest.PatientId,
                DoctorId = doctorTest.DoctorId,
                Specialty = "Neurología",
                AppointmentDate = DateOnly.FromDateTime(DateTime.Now),
                AppointmentHour = new TimeOnly(15, 30)
            };

            //Act

            string tokenString = _tokenService.GenerateToken(patientTest.Name, patientTest.Email, patientTest.Role, patientTest.PatientId);

            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenString);

            var appointmentFormData = AuxMethods.ConvertClassObjectToFormUrlEncoded(appointment);

            var response = await _httpClient.PostAsync("/Appointment/ReserveAppointment", appointmentFormData);

            //Assert

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }


    }
}
