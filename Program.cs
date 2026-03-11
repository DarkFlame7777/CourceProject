using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Services.Abstractions;
using MyProject.Services.Implementions;
using MyProject.Validators;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace MyProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Services.AddControllersWithViews();

            //builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordValidator>();

            //builder.Services.AddFluentValidationAutoValidation(config =>
            //{
            //    config.DisableBuiltInModelValidation = false;
            //    config.EnableFormBindingSourceAutomaticValidation = true;
            //    config.EnableBodyBindingSourceAutomaticValidation = true;
            //    config.EnableQueryBindingSourceAutomaticValidation = true;
            //    config.OverrideDefaultResultFactoryWith<MvcResultFactory>();
            //});

            builder.Services.AddAuthentication("Cookies")
                .AddCookie(options =>
                {
                    options.Cookie.Name = "CourseProjectAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(14);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddScoped<IEmailService, ResendEmailService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
