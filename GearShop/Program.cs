using System.Security.Claims;
using System.Text;
using Azure.Core;
using GearShop.Contracts;
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

namespace GearShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Добавить нормальную обработку версий!
            Console.WriteLine("Version 5");
			var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

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

   //         builder.Services
			//	.AddAuthentication(o =>
			//	{
			//		o.DefaultScheme = "Application";
			//		o.DefaultSignInScheme = "External";
			//	})
			//	.AddCookie("Application")
			//	.AddCookie("External")
			//	.AddGoogle(googleOptions =>
			//	{
	  //          googleOptions.ClientId = config["GoogleOAuth:ClientId"];

			//	googleOptions.ClientSecret = config["GoogleOAuth:ClientSecret"];
	  //          googleOptions.SaveTokens = true;
			//	// googleOptions.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
			//	googleOptions.CallbackPath = new PathString("/ProductList");
			//	googleOptions.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
	  //          googleOptions.ClaimActions.Clear();
	  //          googleOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
	  //          googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
	  //          googleOptions.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
	  //          googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
	  //          googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
	  //          googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
	  //          googleOptions.ClaimActions.MapJsonKey("picture", "picture");
			//});

   //         builder.Services.AddAuthentication().AddOAuth("VK", "", options =>
   //         {
	  //          options.ClientId = config["VkOAuth:AppId"];
	  //          options.ClientSecret = config["VkOAuth:AppSecret"];
	  //          options.ClaimsIssuer = "VKontakte";
	  //          options.CallbackPath = new PathString("/signin-vkontakte-token");
	  //          options.AuthorizationEndpoint = "https://oauth.vk.com/authorize";
	  //          options.TokenEndpoint = "https://oauth.vk.com/access_token";
	  //          options.Scope.Add("email");
	  //          options.SaveTokens = true;
	  //          options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
	  //          options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

	  //          options.Events = new OAuthEvents
   //             {
   //                 OnCreatingTicket = context =>
   //                 {
   //                     context.RunClaimActions(context.TokenResponse.Response.RootElement);
   //                     return Task.CompletedTask;
   //                 },
   //                 OnRemoteFailure = OnFailure
			//	};
   //         });

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
        private static Task OnFailure(RemoteFailureContext arg)
        {
	        Log.Error(arg.Failure.Message);
	        return Task.CompletedTask;
        }
	}
}