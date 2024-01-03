using System.Security.Claims;
using System.Text;
using Azure.Core;
using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using GearShop.Services;
using GearShop.Services.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using static Microsoft.AspNetCore.Authentication.RemoteAuthenticationOptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebMarkupMin.AspNetCore7;

namespace GearShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
			var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            //Добавить нормальную обработку версий!
            Console.WriteLine(config["Version"]);

			builder.Services.AddDbContext<GearShopDbContext>(options =>
              options.UseSqlServer(config["MsSqlConnectionStrings:Default"]), ServiceLifetime.Transient);

			builder.Services.AddTransient<ICryptoService, CryptoService>();
			builder.Services.AddTransient<IGearShopRepository, GearShopRepository>();

			builder.Services.AddTransient<IBackupService, BackupService>();

			builder.Services.AddSingleton<IJwtAuth, JwtAuth>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();

            builder.Services.AddScoped<IVkAuth, VkAuth>();
			
			builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
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

			var provider =builder.Services.BuildServiceProvider();

			builder.Services.AddSingleton<INotifier, Notifier>(x =>
	            new Notifier(config["EmailNotifier:managerEmail"],
		            provider.GetService<IEMailNotifier>(), provider.GetService<ILogger<Notifier>>()));

            builder.Services.AddSingleton<IFileStorage>(x=>new FileStorage("Upload\\Files"));
			builder.Services.AddSingleton<IGoogleAuth, GoogleAuth> ();
			

			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddHttpContextAccessor();

			builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1800);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

			builder.Services.AddScoped<IDataSynchronizer, DataSynchronizer>();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add services to the container.
           builder.Services.AddControllersWithViews()
               .AddRazorRuntimeCompilation(); //Для верстки страниц без перезагрузки сервиса.

            //Сжатие js текста.
                  builder.Services.AddWebMarkupMin(options =>
                  {
                      options.AllowMinificationInDevelopmentEnvironment = true;
                      options.AllowCompressionInDevelopmentEnvironment = true;

                  }).AddHtmlMinification()
                  .AddHttpCompression();


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

			app.UseWebMarkupMin();

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
        private static Task OnFailure(RemoteFailureContext arg)
        {
	        Log.Error(arg.Failure.Message);
	        return Task.CompletedTask;
        }
	}
}