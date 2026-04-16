using HealthManager.Models;
using HealthManager.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HealthManager.Services.Appointments
{
    public interface IAppointments
    {
        public Task<MethodResponse> CheckForExistingAppointments();

        public Task<MethodResponse> CreateAppointmentsForAllDoctors();

        public Task<MethodResponse> CheckAndCreateAppointments();

        public Task<MethodResponse> CreateDoctorAppointments(List<DoctorDTO> doctorList, int dayNumber);

        public Task<MethodResponse> CreateSingleDoctorAppointments(int doctorId);

    }
}
