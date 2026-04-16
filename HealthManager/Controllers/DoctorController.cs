using HealthManager.Models;
using HealthManager.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthManager.Controllers
{
    [Authorize(Roles = "Doctor")]
    
    public class DoctorController : Controller
    {
        private readonly HealthManagerContext _dbcontext;
        public DoctorController(HealthManagerContext context)
        {
            _dbcontext = context;
        }

        public IActionResult Doctors()
        {
            List<Doctor> doctorList = _dbcontext.Doctors.ToList();
            return View(doctorList);
        }

        [HttpGet]
        public async Task<IActionResult> PatientTodayList()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            int.TryParse(userIdString, out int userIdInt);
            var today = DateOnly.FromDateTime(DateTime.Now);
            List<Appointment> patientList = await _dbcontext.Appointments
                .Where(x => x.DoctorId == userIdInt && x.AppointmentDate == today && x.Status == "Reserved" && x.Attended == null )
                .Include(a => a.Patient)
                .OrderBy(x => x.AppointmentHour)
                .ToListAsync();
            return View(patientList);
        }

        public async Task <IActionResult> CreateRecord(Guid appointmentId)
        {
            Appointment appointment = await _dbcontext.Appointments
                .Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .FirstOrDefaultAsync();

            MedicalRecordViewModel record = new MedicalRecordViewModel
            {
                AppointmentId = appointment.AppointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = (int)appointment.PatientId,
                DoctorName = appointment.Doctor.Name + " " + appointment.Doctor.Surname,
                PatientName = appointment.Patient.Name + " " + appointment.Patient.Surname,
            };
            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecord(MedicalRecordViewModel recordViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Guid appointmentId = recordViewModel.AppointmentId;
                    MedicalRecord existingRecord = await _dbcontext.MedicalRecords.Where(x => x.AppointmentId == appointmentId).FirstOrDefaultAsync();
                    if (existingRecord == null)
                    {
                        MedicalRecord newRecord = new MedicalRecord
                        {
                            AppointmentId = appointmentId,
                            DoctorId = recordViewModel.DoctorId,
                            PatientId = recordViewModel.PatientId,
                            Date = DateTime.Now,
                            Diagnosis = recordViewModel.Diagnosis,
                            Observations = recordViewModel.Observations,
                            Treatment = recordViewModel.Treatment,
                        };

                        //EScribir logica para establecer que el paciente asistio a la consulta para que el turno ya no aparezca en la 
                        //Lista de pacientes para hoy
                        await _dbcontext.Appointments
                           .Where(x => x.AppointmentId == newRecord.AppointmentId)
                           .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Attended, true));

                        await _dbcontext.MedicalRecords.AddAsync(newRecord);

                        await _dbcontext.SaveChangesAsync();
                    }

                    return RedirectToAction("PatientTodayList", "Doctor");
                }
                else
                {
                    return View(recordViewModel);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult MedicalRecords()
        {
            return View();
        }

        public async Task<IActionResult> PatientNotAttended(MedicalRecordViewModel recordViewModel)
        {
            try
            {
                Guid appointmentId = recordViewModel.AppointmentId;
                await _dbcontext.Appointments
                        .Where(x => x.AppointmentId == appointmentId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Attended, false));

                await _dbcontext.SaveChangesAsync();

                return RedirectToAction("PatientTodayList");
            }
            catch (Exception)
            {

                return RedirectToAction("PatientTodayList", "Doctor");
            }
        }

        public async Task<IActionResult> GetMedicalRegisters(string query)
        {
            var doctorId = User.FindFirst("Id")?.Value;
            int.TryParse(doctorId, out int doctorIdInt);
            int.TryParse(query, out int patientDni);
            List<MedicalRecordViewModel> recordsList = await _dbcontext.MedicalRecords
                .Where(x => x.DoctorId == doctorIdInt && x.Patient.Dni == patientDni)
                .OrderBy(x => x.Date)
                .Select(x => new MedicalRecordViewModel
                {
                    AppointmentId = (Guid)x.AppointmentId,
                    DoctorId = x.DoctorId,
                    PatientId = x.PatientId,
                    PatientName = x.Patient.Name + " " + x.Patient.Surname,
                    Treatment = x.Treatment,
                    Diagnosis = x.Diagnosis,
                    Observations = x.Observations,
                    RecordDate = (DateTime)x.Date,
                })
                
                .ToListAsync();
            return PartialView("_RecordSearchResults", recordsList);
        }

    }

}
