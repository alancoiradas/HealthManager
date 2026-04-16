using Docker.DotNet.Models;
using HealthManager.Models;
using HealthManager.Models.DTO;
using HealthManager.Services.Appointments;
using HealthManager.Services.Authentication;
using HealthManager.Services.JWTService;
using HealthManagerIntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HealthManagerIntegrationTests.ControllerTests.DoctorTests
{
    [Collection("IntegrationTests")]
    public class DoctorTests : IClassFixture<CustomWebApplicationEnv>
    {
        private HttpClient _httpClient;
        private readonly HealthManagerContext _dbContext;
        private readonly IAppointments _appointmentsService;
        private readonly IJWTService _jwtService;

        public DoctorTests(CustomWebApplicationEnv custom)
        {
            _httpClient = custom.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions 
            {
                AllowAutoRedirect = false
            });
            var scope = custom.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<HealthManagerContext>();
            _appointmentsService = scope.ServiceProvider.GetRequiredService<IAppointments>();
            _jwtService = scope.ServiceProvider.GetRequiredService<IJWTService>();
        }

        [Fact]
        public async Task AppointmentsForADcotorAreSuccessfullyCreated()
        {
            //Arrange

            Doctor doctorTest = _dbContext.Doctors.Where(x => x.DoctorId == 1).Single();
            WorkingDay wd = _dbContext.WorkingDays.Where(x => x.DoctorId == 1).Single();
            DoctorShift ds = _dbContext.DoctorShifts.Where(x => x.DoctorId == 1).Single();

            DoctorDTO dto = new DoctorDTO { Doctor = doctorTest, DoctorShift = ds, WorkingDay = wd};
            List<DoctorDTO> doctorList = new List<DoctorDTO> {dto};


            //Act
            MethodResponse response = await _appointmentsService.CreateDoctorAppointments(doctorList, 1);

            //Assert
            Assert.True(response.Success);


        }

        [Fact]
        public async Task MedicalRegisterIsSuccessfullyCreated()
        {
            //Arrange

            DateOnly todayTest = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly timeTest = new TimeOnly(12,20);

            Patient patientSearch = _dbContext.Patients.Where(x => x.PatientId == 1).Single();

            Appointment appointment = await _dbContext.Appointments
                .Where(x => x.AppointmentDate == todayTest && x.AppointmentHour == timeTest && x.DoctorId == 1)
                .FirstOrDefaultAsync();

            Guid appointmentId = appointment.AppointmentId;

            appointment.PatientId = patientSearch.PatientId;
            appointment.DoctorId = 1;
            appointment.Status = "Reserved";

            _dbContext.Update(appointment);
            await _dbContext.SaveChangesAsync();

            Doctor doctorTest = await _dbContext.Doctors.Where(x => x.DoctorId == 1).FirstOrDefaultAsync();

            string token = _jwtService.GenerateToken(doctorTest.Name, doctorTest.Email, "Doctor", doctorTest.DoctorId);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act

            MedicalRecordViewModel viewModel = new MedicalRecordViewModel
            {
                AppointmentId = appointmentId,
                DoctorId = doctorTest.DoctorId,
                DoctorName = doctorTest.Name + doctorTest.Surname,
                PatientId = patientSearch.PatientId,
                PatientName = patientSearch.Name + patientSearch.Surname,
                Diagnosis = "Test Diagnosis",
                Treatment = "Test Treatment",
                Observations = "Test Observations",
                RecordDate = DateTime.Now,
            };

            var requestFormData = AuxMethods.ConvertClassObjectToFormUrlEncoded(viewModel);

            var response = await _httpClient.PostAsync("/Doctor/CreateRecord", requestFormData);

            //Assert

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

    }
}
