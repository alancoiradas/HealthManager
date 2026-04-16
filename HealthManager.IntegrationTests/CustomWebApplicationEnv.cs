using DotNet.Testcontainers.Builders;
using HealthManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using HealthManager.Services.Authentication;
using HealthManager.Services.JWTService;
using HealthManager.Models.DTO;
using HealthManager.Services.Appointments;
using HealthManagerIntegrationTests.Helpers;

namespace HealthManagerIntegrationTests
{
    public class CustomWebApplicationEnv : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;

        public CustomWebApplicationEnv() 
        {
            _dbContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("YourStrong(!)Password")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("MSSQL_PID", "Express")
                .WithPortBinding(1433, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilMessageIsLogged("SQL Server is now ready for client connections"))
                .Build();
        }
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HealthManagerContext>();
            db.Database.SetCommandTimeout(120);

            await db.Database.EnsureCreatedAsync();

            await SeedMethods.SeedAsync(db);

            var appointmentsService = scope.ServiceProvider.GetService<IAppointments>();
            var response = await appointmentsService.CheckAndCreateAppointments();

        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            
            builder.ConfigureServices(services => 
            {
                services.AddScoped<IJWTService, JWTService>();
                services.AddScoped<IAppointments, AppointmentsService>();
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<HealthManagerContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<HealthManagerContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
                
            });

        }
    }
}
