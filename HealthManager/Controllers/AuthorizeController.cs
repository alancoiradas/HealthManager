using HealthManager.Models;
using HealthManager.Models.DTO;
using HealthManager.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;

namespace HealthManager.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly HealthManagerContext _dbcontext;
        private readonly IJWTService _jwtservice;
        public AuthorizeController(HealthManagerContext context, IJWTService jwtservice) {
            _dbcontext = context;
            _jwtservice = jwtservice;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            var existingCookie = ControllerContext.HttpContext.Request.Cookies["Token"];
            if (existingCookie != null)
            {
                string role = User.FindFirst(ClaimTypes.Role).Value;
                switch (role)
                {
                    case "Patient":
                        return RedirectToAction("MyAppointments", "PatientDashboard");
                        
                    case "Doctor":
                        return RedirectToAction("PatientTodayList", "Doctor");
                    case "Admin":
                       return RedirectToAction("EmployeeList", "Admin");
                    default:
                        break;
                }
            }
           
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Login(AuthorizeRequest request)
        {
           if (ModelState.IsValid)
           {
                Patient patientSearch = await _dbcontext.Patients.Where(p => p.Email == request.Email).FirstOrDefaultAsync();
                if (patientSearch == null)
                {
                    ModelState.AddModelError("Email", "Cannot find an account with the provided email.");
                    return View(request);
                }
                if (!BCrypt.Net.BCrypt.Verify(request.Password, patientSearch.Password))
                {
                    ViewData["Credentials"] = "The credentials are invalid. Please try again.";
                    return View(request);
                }
                string createdToken = _jwtservice.GenerateToken(patientSearch.Name, patientSearch.Email, patientSearch.Role, patientSearch.PatientId);
                HttpContext.Response.Cookies.Append("Token", createdToken,
                    new CookieOptions
                    {
                        Path = "/",
                        Expires = DateTime.UtcNow.AddDays(1),
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.None,
                    });
                return RedirectToAction("MyAppointments", "PatientDashboard");
           }  
           else
           {
                return View(request);
           }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() 
        {
           var existingCookie = ControllerContext.HttpContext.Request.Cookies["Token"];
           if (existingCookie != null)
           {
                string role = User.FindFirst(ClaimTypes.Role).Value;
                switch (role)
                {
                    case "Patient":
                        return RedirectToAction("MyAppointments", "PatientDashboard");

                    case "Doctor":
                        return RedirectToAction("PatientTodayList", "Doctor");
                    case "Admin":
                        return RedirectToAction("EmployeeList", "Admin");
                    default:
                        break;
                }
            }
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Register(PatientDTO patientData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Patient checkPatientExists = _dbcontext.Patients.FirstOrDefault(x => x.Email == patientData.Email);
                    if (checkPatientExists == null)
                    {
                        Patient newPatient = new Patient
                        {
                            Name = patientData.Name,
                            Surname = patientData.Surname,
                            BirthDate = patientData.Birthdate,
                            Email = patientData.Email,
                            Password = BCrypt.Net.BCrypt.HashPassword(patientData.Password),
                            Dni = patientData.Dni,
                            PhoneNumber = patientData.PhoneNumber,
                            Gender = patientData.Gender,
                            Sex = patientData.Sex,
                        };
                        await _dbcontext.Patients.AddAsync(newPatient);
                        await _dbcontext.SaveChangesAsync();
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "* The provided email is already in use.");
                        return View(patientData);
                    }
                }
                else
                {
                    ViewData["ErrorMessage"] = "* There are missing or invalid fields on the form.";
                    return View(patientData);
                }
            }
            catch (Exception error)
            {

                ModelState.AddModelError(string.Empty, "* An error occurred while processing your request. Please try again.");
                Console.WriteLine(error);
                return View(patientData);
            }
        }
        public async Task <IActionResult> Logout()
        {
            var token = Request.Cookies["Token"];
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if(role == "Admin" || role == "Doctor")
            {
                return RedirectToAction("AdminLogin");
            }
            else
            {
                return RedirectToAction("Login");
            }
                
        }

        [AllowAnonymous]
        public IActionResult AdminLogin()
        {
            var Role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (Role == null)
            {
                return View();
            }
            else if(Role == "Admin")
            {
                return RedirectToAction("EmployeeList", "Admin");
            }
            else
            {
                return RedirectToAction("PatientTodayList", "Doctor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(AuthorizeRequest request)
        {
            Doctor employee = await _dbcontext.Doctors.Where(d => d.Email == request.Email).FirstOrDefaultAsync(); 
            Admin admin = await _dbcontext.Admins.Where(d => d.Email == request.Email).FirstOrDefaultAsync();
            
           if (employee == null && admin != null)
            {
               
                if (BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
                {
                    string adminToken = _jwtservice.GenerateToken(admin.Name, admin.Email, admin.Role, admin.Id);
                    HttpContext.Response.Cookies.Append("Token", adminToken,
                        new CookieOptions
                        {
                            Path = "/",
                            Expires = DateTime.UtcNow.AddDays(1),
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true,
                            SameSite = SameSiteMode.None,
                        });
                    return RedirectToAction("CreateAdmin", "Admin");
                    
                }
                else
                {
                    ViewData["Credentials"] = "The credentials are invalid. Please try again.";
                    return View(request);
                }
            } else if (employee != null && admin == null)
            {
                
                if (BCrypt.Net.BCrypt.Verify(request.Password, employee.Password))
                {
                    string employeeToken = _jwtservice.GenerateToken(employee.Name, employee.Email, employee.Role, employee.DoctorId);
                    HttpContext.Response.Cookies.Append("Token", employeeToken,
                        new CookieOptions
                        {
                            Path = "/",
                            Expires = DateTime.UtcNow.AddDays(1),
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true,
                            SameSite = SameSiteMode.None,
                        });
                    return RedirectToAction("PatientTodayList", "Doctor");
                }
                else
                {
                    ViewData["Credentials"] = "The credentials are invalid. Please try again.";
                    return View(request);
                }
            }
            ModelState.AddModelError("Email", "* We couldn't find an employee account with the provided email.");
            return View(request);
            
           
           
            
        }
    }
}
