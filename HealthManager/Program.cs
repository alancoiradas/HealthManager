using HealthManager.Models;
using HealthManager.Services.Appointments;
using HealthManager.Services.Authentication;
using HealthManager.Services.JWTService;
using HealthManager.Services.Mail;
using HealthManager.Services.PDF.AppointmentReceipt;
using HealthManager.Services.Seed;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;

public class Program 
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);



        // Add services to the container.

        builder.Services.AddHostedService<AppointmentsBackgroundTask>();
        builder.Services.AddHostedService<AppointmentsLifeCicle>();
        builder.Services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });

        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<HealthManagerContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("HealthManager"));
        });

        //Añadir dependencias

        builder.Services.AddScoped<IJWTService, JWTService>();
        builder.Services.AddScoped<IAppointments, AppointmentsService>();
        builder.Services.AddScoped<IMailService, MailService>();
        builder.Services.AddScoped<IAppointmentReceipt, AppointmentReceiptService>();

        //Configuración de autenticación

        var tokenKey = builder.Configuration.GetSection("JWT").GetSection("secret-key").ToString();

        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddCookie(c =>
            {
                c.Cookie.Name = "Token";
            })
        .AddJwtBearer(config =>
        {
            config.RequireHttpsMetadata = false;
            config.SaveToken = false;
            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
            };
            config.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["Token"];
                    return Task.CompletedTask;
                }
            };
        });

        /*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(option =>
        {
            option.LoginPath = "/Authorize/Login";
            option.ExpireTimeSpan = TimeSpan.FromDays(7);
            option.AccessDeniedPath = "/Authorize/Login";
        });*/

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
            .SetApplicationName("HealthManager");

        QuestPDF.Settings.License = LicenseType.Community;

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

}