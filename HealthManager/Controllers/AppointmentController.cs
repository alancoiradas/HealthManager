using HealthManager.Models;
using HealthManager.Models.DTO;
using HealthManager.Services.Appointments;
using HealthManager.Services.Mail;
using HealthManager.Services.PDF.AppointmentReceipt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HealthManager.Controllers
{
    [Authorize(Roles = "Patient")]
    public class AppointmentController : Controller
    {
        private readonly HealthManagerContext _dbcontext;
        private readonly IAppointments _appointmentsService;
        private readonly IAppointmentReceipt _appointmentReceipt;
        private readonly IMailService _mailService;
        public AppointmentController(HealthManagerContext context,
            IAppointments appointmentsService,
            IAppointmentReceipt appointmentReceipt,
            IMailService mailService)
        {
            _dbcontext = context;
            _appointmentsService = appointmentsService;
            _appointmentReceipt = appointmentReceipt;
            _mailService = mailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task <IActionResult> ReserveAppointment()
        {
            DateTime today = DateTime.Now;
            DateOnly day = DateOnly.FromDateTime(today);
            TimeOnly hours = TimeOnly.FromDateTime(today);
            var appointmentsList = await _dbcontext.Appointments
                .Where(a => a.Status == "Available" && a.AppointmentDate> day)
                .Select(a => new AppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                })
                .ToListAsync();
            ViewData["AppointmentsAvailable"] = new SelectList(appointmentsList, "AppointmentId", "AppointmentTime");

            var specialties = await _dbcontext.Specialties
                .Where(x => x.Doctors.Count() >  0)
                .Distinct().ToListAsync();
            ViewData["Specialties"] = new SelectList(specialties, "SpecialtyId", "SpecialtyName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReserveAppointment(AppointmentViewModel appointmentRequest)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("Id")?.Value;
                int.TryParse(userId, out int userIdInt);
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                int todayInt = today.Day;
                TimeOnly currentHour = TimeOnly.FromDateTime(DateTime.Now);
                var existingAppointment = await _dbcontext.Appointments
                    .Where(x =>  x.DoctorId == appointmentRequest.DoctorId 
                                && x.PatientId == userIdInt
                                && (x.AppointmentDate.Month == appointmentRequest.AppointmentDate.Month 
                                    || x.AppointmentDate.Month == appointmentRequest.AppointmentDate.AddMonths(1).Month)
                                    && (x.AppointmentDate.Day > todayInt || (x.AppointmentDate == today && x.AppointmentHour > currentHour))
                                && x.Status == "Reserved"
                                && x.Attended == null)
                    .FirstOrDefaultAsync();


                if (existingAppointment != null)
                {
                    ViewData["Appointment"] = "There's already an existing appointment for this patient. " +
                            "If you want to set another appointment, please cancel the existing one first";
                        
                            
                    return View();
                }
                
                Appointment reserveAppointment = await _dbcontext.Appointments.Where(x => x.DoctorId == appointmentRequest.DoctorId
                                && x.AppointmentDate == appointmentRequest.AppointmentDate
                                && x.AppointmentHour == appointmentRequest.AppointmentHour
                                && x.Status == "Available").FirstOrDefaultAsync();
                if (reserveAppointment != null)
                {
                    reserveAppointment.Status = "Reserved";
                    reserveAppointment.PatientId = userIdInt;
                    _dbcontext.Appointments.Update(reserveAppointment);
                    await _dbcontext.SaveChangesAsync();

                    var patientData = _dbcontext.Patients.Where(x => x.PatientId == userIdInt).Select(x => new
                    {
                        PatientEmail = x.Email,
                        PatientName = x.Name + " " + x.Surname,
                    }).FirstOrDefault();

                    var doctorData = _dbcontext.Doctors.Where(x => x.DoctorId == appointmentRequest.DoctorId).Select(x => new
                    {
                        FullName = $"{x.Name} {x.Surname}",
                        Specialty = x.SpecialtyNavigation.SpecialtyName,

                    }).FirstOrDefault();

                    AppointmentDataPDFDTO appointmentData = new AppointmentDataPDFDTO
                    {
                        PatientName = patientData.PatientName,
                        DoctorName = doctorData.FullName,
                        AppointmentDate = reserveAppointment.AppointmentDate,
                        AppointmentHour = reserveAppointment.AppointmentHour,
                        Specialty = doctorData.Specialty

                    };

                    var pdfByte = _appointmentReceipt.CreateAppointmentReceipt(appointmentData);

                    

                    MailDTO mailSample = new MailDTO
                    {
                        DestinataryMail = patientData?.PatientEmail,
                        DestinataryName = patientData?.PatientName,
                        MailSubject = $"Medical appointment requested at Healthmanager.",
                        MailTitle = "Appointment confirmation.",
                    };

                    _mailService.SendAppointmentConfirmationMail(mailSample, pdfByte);

                }

                return RedirectToAction("MyAppointments", "PatientDashboard");
            }

            return View(appointmentRequest);
        }

        
        public async Task <JsonResult> GetAppointmentDates(int doctorId)
        {
            var today = DateTime.Now;
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateOnly.FromDateTime(today);
            var currentHour = TimeOnly.FromDateTime(DateTime.Now).AddHours(1);
            var availableAppointments =  _dbcontext.Appointments
                .Where(a => a.DoctorId == doctorId && a.Status == "Available")
                .AsEnumerable()
                .Where(a => a.AppointmentDate.CompareTo(currentDay) > 0 || a.AppointmentDate.ToDateTime(a.AppointmentHour) > today)
                .Select(a => a.AppointmentDate)
                .Distinct()
                .ToList();

            var orderedList = availableAppointments
                .Select( a => a.ToString("dd/MM/yyyy"))
                .OrderBy(x => DateTime.ParseExact(x, "dd/MM/yyyy", null));
            return Json(orderedList);
        }

        public async Task <JsonResult> GetAppointmentHours(string day, int doctorId)
        {
            var now = DateTime.Now;
            var dateFromString = DateTime.Parse(day);
            var onlyDateFromDateTime = DateOnly.FromDateTime(dateFromString);
            var currentHour = TimeOnly.FromDateTime(DateTime.Now).AddHours(1);
            var today = DateOnly.FromDateTime(now);

            List<TimeOnly> appointmentHours = new List<TimeOnly>();
            List<string> orderedList = new List<string>();

            if (onlyDateFromDateTime.CompareTo(today) > 0)
            {
                 appointmentHours = await _dbcontext.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Equals(onlyDateFromDateTime) && a.Status == "Available")
                .Select(a => a.AppointmentHour)
                .ToListAsync();
                orderedList = appointmentHours.Select(a => a.ToString("HH:mm")).OrderBy(x => TimeOnly.ParseExact(x, "HH:mm", null)).ToList();
            }
            else
            {
                 appointmentHours = await _dbcontext.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Equals(onlyDateFromDateTime) && a.AppointmentHour >= currentHour && a.Status == "Available")
                .Select(a => a.AppointmentHour)
                .ToListAsync();
                orderedList = appointmentHours.Select(a => a.ToString("HH:mm")).OrderBy(x => TimeOnly.ParseExact(x, "HH:mm", null)).ToList();
            }
                return Json(orderedList);
        }

        public async Task<JsonResult> GetDoctorsBySpecialty(int specialty)
        {
            var doctorsBySpecialty = await _dbcontext.Doctors.Where(d => d.Specialty == specialty)
                .Select(a => new
                {
                    DoctorId = a.DoctorId,
                    Name = a.Name + " " + a.Surname,
                })
                .ToListAsync();
            return Json(doctorsBySpecialty);
        }
    }
}
