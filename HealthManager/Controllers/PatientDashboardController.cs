using HealthManager.Models;
using HealthManager.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HealthManager.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientDashboardController : Controller
    {
        private readonly HealthManagerContext _dbcontext;

        public PatientDashboardController(HealthManagerContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task <IActionResult> MyAppointments()
        {
            /* var userCookie = Request.Cookies["User"];

             if (string.IsNullOrEmpty(userCookie))
             {
                  return RedirectToAction("Login", "Authorize");
             }


             if (!int.TryParse(userCookie, out int patientId))
             {
                 return BadRequest("Invalid cookie value. There is no cookie.");
             }

             var patientAppointments = await _dbcontext.Appointments
                 .Where(p => p.PatientId == patientId && p.Status=="Reserved")
                 .ToListAsync();*/
            DateTime today = DateTime.Now;
            DateOnly todayDate = DateOnly.FromDateTime(today);
            TimeOnly todayhour = TimeOnly.FromDateTime(today);
            var userIdString = User.FindFirst("Id")?.Value;
            int.TryParse(userIdString, out int userIdInt);
            var patientAppointments = await _dbcontext.Appointments
                 .Where(p => p.Status == "Reserved" && p.PatientId == userIdInt && (p.AppointmentDate > todayDate || (p.AppointmentDate == todayDate && p.AppointmentHour > todayhour)))
                 .Include(a => a.Doctor)
                 .ThenInclude(d => d.SpecialtyNavigation)
                 .OrderByDescending(x => x.AppointmentDate)
                 .ThenByDescending(x => x.AppointmentHour)
                 .ToListAsync();
            List<PatientAppointmentsViewModel> appointmentsVm = patientAppointments.Select(x => new PatientAppointmentsViewModel
            {
                AppointmentId = x.AppointmentId,
                DoctorName = x.Doctor.Name + " " + x.Doctor.Surname,
                DoctorSpecialty = x.Doctor.SpecialtyNavigation.SpecialtyName,
                AppointmentDate = DateOnly.ParseExact(x.AppointmentDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                AppointmentHour = x.AppointmentHour
            }).ToList();

            return View(appointmentsVm);
        }
        
        [HttpPost]
        public async Task<MethodResponse> CancelAppointment(Guid appointmentId)
        {
            Appointment databaseAppointment = await _dbcontext.Appointments.FindAsync(appointmentId);
            var requestDate = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly requestTime = TimeOnly.FromDateTime(DateTime.Now);

            DateTime date = DateTime.Now;

            if (databaseAppointment.AppointmentDate.CompareTo(requestDate) >= 0 && date < databaseAppointment.AppointmentDate.ToDateTime(databaseAppointment.AppointmentHour))
            {
                databaseAppointment.Status = "Available";
                databaseAppointment.PatientId = null;
                _dbcontext.Appointments.Update(databaseAppointment);
                await _dbcontext.SaveChangesAsync();
                MethodResponse response = new MethodResponse 
                {
                    Success = true,
                    Message = ""
                };
                return response;
            }
            else
            {
                ViewData["CancelError"] = "The appoinment has to be cancelled before the appointment date.";
                MethodResponse response = new MethodResponse
                {
                    Success = false,
                    Message = ""
                };
                return response;
            }
        }
    }
}
