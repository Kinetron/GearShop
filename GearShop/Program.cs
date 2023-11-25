using System.Text;
using Azure.Core;
using GearShop.Contracts;
using GearShop.Services;
using GearShop.Services.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

namespace GearShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Добавить нормальную обработку версий!
            Console.WriteLine("Version 3");
			var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                //x.TokenValidationParameters = new TokenValidationParameters()
                //{
                //    ValidIssuer = config["JwtSettings:Issuer"],
                //    ValidAudience = config["JwtSettings:Audience"],
                //    IssuerSigningKey = new SymmetricSecurityKey(
                //        Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!)),
                //    ValidateIssuer = false,
                //    ValidateAudience = false,
                //    ValidateLifetime = true,
                //    ValidateIssuerSigningKey = true
                //};

                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!)),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            builder.Services.AddDetection(); //Определение типа устройства.

			builder.Services.AddSingleton<IEMailNotifier, EMailNotifier>(x=> 
	            new EMailNotifier(config["EmailNotifier:senderEmail"],
		            config["EmailNotifier:senderPassword"],
		            config["EmailNotifier:companyName"]));
            
			builder.Services.AddSingleton<INotifier, Notifier>(x =>
	            new Notifier(config["EmailNotifier:managerEmail"], 
		            builder.Services.BuildServiceProvider().GetService<IEMailNotifier>()));

            builder.Services.AddSingleton<IFileStorage>(x=>new FileStorage("Upload\\Files"));
            builder.Services.AddScoped<IIdentityService>(x => 
                new IdentityService(config["JwtSettings:Key"]!, x.GetRequiredService<IGearShopRepository>()));
            
            builder.Services.AddDistributedMemoryCache();
			builder.Services.AddHttpContextAccessor();

			builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1800);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            
            builder.Services.AddDbContext<GearShopDbContext>(options =>
                options.UseSqlServer(config["MsSqlConnectionStrings:Default"]));

            builder.Services.AddScoped<IDataSynchronizer, DataSynchronizer>();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddScoped<IGearShopRepository, GearShopRepository>();

            // Add services to the container.
           builder.Services.AddControllersWithViews()
               .AddRazorRuntimeCompilation(); //Для верстки страниц без перезагрузки сервиса.


           Log.Logger = new LoggerConfiguration()
	           .MinimumLevel.Information()
	           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) //Выводить только варнинги Microsoft.
	           .WriteTo.File(
		           @"./logs/log.txt",
		           shared: true, //Доступен всем процессам.
		           rollingInterval: RollingInterval.Day,
		           flushToDiskInterval: TimeSpan.FromSeconds(20),
		           outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
	           .CreateLogger();

			builder.Host.UseSerilog();
			builder.Services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			});

			var app = builder.Build();
			

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

			app.UseSerilogRequestLogging();

			app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseDetection(); //Определение типа устройства.
			app.UseSession();

            app.Use(async (context, next) =>
            {
                var JWToken = context.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(JWToken))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
                }
                await next();
            });
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}