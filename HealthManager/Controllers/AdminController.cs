using HealthManager.Models.DTO;
using HealthManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using HealthManager.Services.Appointments;

namespace HealthManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly HealthManagerContext _dbcontext;
        private readonly IAppointments _appointmentsService;

        public AdminController(HealthManagerContext context, IAppointments appointmentsService)
        {
            _dbcontext = context;
            _appointmentsService = appointmentsService;
        }
        public async Task <IActionResult> EmployeeList()
        {
            List<Admin> adminList = await _dbcontext.Admins.ToListAsync();
            List<Doctor> doctorList = await _dbcontext.Doctors.ToListAsync();

            EmployeeViewModel employeeViewModel = new EmployeeViewModel
            {
                AdminList = adminList,
                DoctorList = doctorList,
            };
            /*Esta vista devolverá la lista de empleados de la db con su rol, y organizados por este*/
            return View(employeeViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            List<Specialty> specialtyList = await _dbcontext.Specialties.ToListAsync();
            
            var specialtyOrderList = specialtyList.OrderBy(x => x.SpecialtyName);
            ViewData["Specialties"] = new SelectList(specialtyOrderList, "SpecialtyId", "SpecialtyName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(DoctorRegistrationViewModel doctorRequest)
        {

            using var transaction = await _dbcontext.Database.BeginTransactionAsync();
            try
            {
                Specialty newDoctorSpecialty = await _dbcontext.Specialties.Where(x => x.SpecialtyId == doctorRequest.Specialty).FirstOrDefaultAsync();
                TimeOnly consultDuration = new TimeOnly().AddMinutes(doctorRequest.ConsultDuration);
                Doctor newDoctor = new Doctor
                {
                    Name = doctorRequest.Name,
                    Surname = doctorRequest.Surname,
                    Specialty = newDoctorSpecialty.SpecialtyId,
                    Email = doctorRequest.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(doctorRequest.Password),
                };
                await _dbcontext.Doctors.AddAsync(newDoctor);
                await _dbcontext.SaveChangesAsync();

                WorkingDay newDoctorSchedule = new WorkingDay
                {
                    DoctorId = newDoctor.DoctorId,
                    Monday = doctorRequest.Monday,
                    Tuesday = doctorRequest.Tuesday,
                    Wednesday = doctorRequest.Wednesday,
                    Thursday = doctorRequest.Thursday,
                    Friday = doctorRequest.Friday,
                    Saturday = doctorRequest.Saturday,
                    Sunday = doctorRequest.Sunday,
                };
                await _dbcontext.WorkingDays.AddAsync(newDoctorSchedule);
                await _dbcontext.SaveChangesAsync();

                DoctorShift newAppointmentInfo = new DoctorShift
                {
                    DoctorId = newDoctor.DoctorId,
                    ShiftStart = doctorRequest.WorkingHoursStart,
                    ShiftEnd = doctorRequest.WorkingHoursEnd,
                    ConsultDuration = consultDuration,
                };

                await _dbcontext.DoctorShifts.AddAsync(newAppointmentInfo);
                await _dbcontext.SaveChangesAsync();
                
                await transaction.CommitAsync();

                var response = await _appointmentsService.CreateSingleDoctorAppointments(newAppointmentInfo.DoctorId);

                return View();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return View(doctorRequest);
            }
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(AdminRegisterViewModel adminModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Admin adminSearch = await _dbcontext.Admins.Where(x => x.Email == adminModel.Email).FirstOrDefaultAsync();

                    if (adminSearch == null)
                    {
                        Admin newAdmin = new Admin
                        {
                            Name = adminModel.FirstName,
                            Surname = adminModel.LastName,
                            Email = adminModel.Email,
                            Password = BCrypt.Net.BCrypt.HashPassword(adminModel.Password),

                        };
                        await _dbcontext.Admins.AddAsync(newAdmin);
                        await _dbcontext.SaveChangesAsync();
                        return RedirectToAction("CreateAdmin");
                    }
                    else
                    {
                        return View(adminModel);
                    }
                }
                else
                {
                    return View(adminModel);
                }
            }
            catch (Exception)
            {

                return View(adminModel);
            }
        }
    }
}
