using System.Globalization;
using System.Text;
using GearShop.Contracts;
using GearShop.Services;
using GearShop.Services.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using WebMarkupMin.AspNetCore7;
using Microsoft.Extensions.Options;
using System.Linq;

namespace GearShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
			var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            Console.WriteLine(config["Version"]); //Change to a normal implementation

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
			builder.Services.AddDbContext<GearShopDbContext>(options =>
              options.UseSqlServer(config["MsSqlConnectionStrings:Default"]), ServiceLifetime.Transient);

			builder.Services.AddTransient<ICryptoService, CryptoService>();
			builder.Services.AddTransient<IGearShopRepository, GearShopRepository>();
            
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

			builder.Services.AddDetection(); //Device type detection.

			builder.Services.AddSingleton<IEMailNotifier, EMailNotifier>(x=> 
	            new EMailNotifier(config["EmailNotifier:senderEmail"],
		            config["EmailNotifier:senderPassword"],
		            config["EmailNotifier:companyName"]));

			var provider = builder.Services.BuildServiceProvider();

			builder.Services.AddSingleton<INotifier, Notifier>(x =>
	            new Notifier(config["EmailNotifier:managerEmail"],
		            provider.GetService<IEMailNotifier>(), provider.GetService<ILogger<Notifier>>()));

			if (!bool.TryParse(config["DbBackupSettings:AllowDownloadDbBackup"], out bool allowDownloadDbBackup))
			{
				throw new ArgumentException("Bad param for DbBackupSettings:AllowDownloadDbBackup");
			}

			if (string.IsNullOrEmpty(config["DbBackupSettings:AllowedIp"]))
			{
				throw new ArgumentException("Bad param for DbBackupSettings:AllowedIp");
			}

			builder.Services.AddTransient<IBackupService>(x => new BackupService(allowDownloadDbBackup,
				config["DbBackupSettings:PathToDbBackupFiles"],
				provider.GetService<IGearShopRepository>()));


			builder.Services.AddSingleton<IFileStorage>(x=>new FileStorage("Upload\\Files"));
			builder.Services.AddSingleton<IGoogleAuth, GoogleAuth> ();
			

			builder.Services.AddDistributedMemoryCache();

			builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1800);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

			builder.Services.AddScoped<IDataSynchronizer, DataSynchronizer>();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

           //Add services to the container.
           builder.Services.AddControllersWithViews()
               .AddRazorRuntimeCompilation() //For page debug without restarting the service.
               .AddViewLocalization() //Localization of views
			   .AddDataAnnotationsLocalization(); // Localization of validation attributes

			//JS text compression.
			builder.Services.AddWebMarkupMin(options =>
                  {
                      options.AllowMinificationInDevelopmentEnvironment = true;
                      options.AllowCompressionInDevelopmentEnvironment = true;

                  }).AddHtmlMinification()
                  .AddHttpCompression();


            Log.Logger = new LoggerConfiguration()
	           .MinimumLevel.Information()
	           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	           .WriteTo.File(
		           @"./logs/log.txt",
		           shared: true, //Available to all processes.
				   rollingInterval: RollingInterval.Day,
		           flushToDiskInterval: TimeSpan.FromSeconds(20),
		           outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
	           .CreateLogger();

			builder.Host.UseSerilog();
			builder.Services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			});

			string defaultCulture = config["Globalization:DefaultCulture"];
			builder.Services.Configure<RequestLocalizationOptions>(options =>
			{
				var cultures = new[]
				{
					new CultureInfo(defaultCulture)
				};

				options.DefaultRequestCulture = new RequestCulture(defaultCulture);
				options.SupportedCultures = cultures;
				options.SupportedUICultures = cultures;
			});

			var app = builder.Build();

			app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

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

            app.UseDetection();
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