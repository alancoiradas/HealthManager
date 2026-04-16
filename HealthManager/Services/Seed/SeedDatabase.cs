using HealthManager.Models;
using HealthManager.Services.Appointments;

namespace HealthManager.Services.Seed
{
    public class SeedInitialData
    {
        private readonly HealthManagerContext _dbcontext;
        private readonly IAppointments _appointments;
        private readonly IConfiguration _configuration;
        public SeedInitialData(HealthManagerContext context, IAppointments appointments, IConfiguration configuration)
        {
            _appointments = appointments;
            _dbcontext = context;
            _configuration = configuration;
        }


        List<Specialty> specialtyList = new List<Specialty>
        {
            new Specialty{SpecialtyName= "Cardiology"},
            new Specialty{SpecialtyName= "Dermatology"},
            new Specialty{SpecialtyName= "Pediatrics"},
            new Specialty{SpecialtyName= "Neurology"},
            new Specialty{SpecialtyName= "Orthopedics"},
            new Specialty{SpecialtyName= "Oftalmology"},
            new Specialty{SpecialtyName= "Radiology"},
            new Specialty{SpecialtyName= "Psychiatry"},
            new Specialty{SpecialtyName= "Gastroenterology"},
            new Specialty{SpecialtyName= "Endocrinology"},
            new Specialty{SpecialtyName= "Pulmonology"},
            new Specialty{SpecialtyName= "Oncology"},
            new Specialty{SpecialtyName= "Nephrology"},
            new Specialty{SpecialtyName= "Rheumatology"},
            new Specialty{SpecialtyName= "Urology"},
        };


        

       


        public void SeedDatabase()
        {
            
            try 
            {
                List<Doctor> doctorList = new List<Doctor>
                {
                    new Doctor
                    {
                        Name= "Pedro",
                        Surname= "Gutierrez",
                        Specialty= 1,
                        Email = _configuration.GetSection("InitialData").GetSection("Mails").GetSection("Doctor1").Value,
                        Password = BCrypt.Net.BCrypt.HashPassword(_configuration.GetSection("InitialData").GetSection("Passwords").GetSection("Doctor1").Value)
                    },
                    new Doctor
                    {
                        Name= "Andrea",
                        Surname= "Rodriguez",
                        Specialty= 3,
                        Email =_configuration.GetSection("InitialData").GetSection("Mails").GetSection("Doctor2").Value,
                        Password = BCrypt.Net.BCrypt.HashPassword(_configuration.GetSection("InitialData").GetSection("Passwords").GetSection("Doctor2").Value)
                    },
                    new Doctor
                    {
                        Name= "Gustavo",
                        Surname= "Benitez",
                        Specialty= 4,
                        Email =_configuration.GetSection("InitialData").GetSection("Mails").GetSection("Doctor3").Value,
                        Password = BCrypt.Net.BCrypt.HashPassword(_configuration.GetSection("InitialData").GetSection("Passwords").GetSection("Doctor3").Value)
                    },
                };

                if (!_dbcontext.Specialties.Any())
                {
                    foreach (var specialty in specialtyList)
                    {
                        _dbcontext.Specialties.Add(specialty);
                        _dbcontext.SaveChanges();
                    }
                    
                }

                _dbcontext.SaveChanges();

                if (!_dbcontext.Doctors.Any())
                {
                    
                        _dbcontext.Doctors.AddRange(doctorList);
                        _dbcontext.SaveChanges();
                    List<Doctor> databaseDoctors = _dbcontext.Doctors.ToList();




                    List<DoctorShift> shiftList = new List<DoctorShift>
        {
            new DoctorShift
            {
                DoctorId = databaseDoctors[0].DoctorId,
                ShiftStart = new TimeOnly(09,00),
                ShiftEnd = new TimeOnly(16,00),
                ConsultDuration = new TimeOnly(00, 15)
            },
            new DoctorShift
            {
                DoctorId = databaseDoctors[1].DoctorId,
                ShiftStart = new TimeOnly(10,00),
                ShiftEnd = new TimeOnly(18,00),
                ConsultDuration = new TimeOnly(00, 30)
            },
            new DoctorShift
            {
                DoctorId = databaseDoctors[2].DoctorId,
                ShiftStart = new TimeOnly(09,45),
                ShiftEnd = new TimeOnly(16,30),
                ConsultDuration = new TimeOnly(00, 15)
            }
        };

                    List<WorkingDay> woringDayList = new List<WorkingDay>
        {
            new WorkingDay{
                DoctorId = databaseDoctors[0].DoctorId,
                Monday = true,
                Tuesday = false,
                Wednesday = true,
                Thursday = false,
                Friday = true,
                Saturday = false,
                Sunday = false
            },

            new WorkingDay
            {
                DoctorId = databaseDoctors[1].DoctorId,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                Sunday = false
            },

            new WorkingDay
            {
                DoctorId = databaseDoctors[2].DoctorId,
                Monday = false,
                Tuesday = true,
                Wednesday = false,
                Thursday = true,
                Friday = false,
                Saturday = true,
                Sunday = false
            }
        };

                   
                        _dbcontext.WorkingDays.AddRange(woringDayList);
                        _dbcontext.SaveChanges();
                    

                        _dbcontext.DoctorShifts.AddRange(shiftList);
                        _dbcontext.SaveChanges();
                    

                    _dbcontext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SeedDatabase: {ex.Message}");
                throw;
            }

            
            
        }
    }
}
