using System;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Models.Converters;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Filters;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;
using AuthenticationService = SolidTradeServer.Services.AuthenticationService;

namespace SolidTradeServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            
            services.AddDbContext<DbSolidTrade>();
            
            services.AddTransient<UserService>();
            services.AddTransient<WarrantService>();
            services.AddTransient<AuthenticationService>();
            
            services.AddLogging();

            services.AddControllers(options =>
            {
                options.Filters.Add<AuthenticationFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new StringRemoveWhitespaceConverter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseExceptionHandler(a => a.Run(async httpContext =>
            {
                var exceptionHandlerPathFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
                var e = exceptionHandlerPathFeature.Error;

                var result = JsonConvert.SerializeObject(new UnexpectedError
                {
                    Title = "Unexpected error",
                    Message = e.Message,
                    Exception = env.IsDevelopment() ? e : null,
                });
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(result).ConfigureAwait(false);
            }));
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(Configuration["FirebaseCredentials"]),
            });
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Configuration["FirebaseCredentials"]);
            
            CommonService.Firestore = FirestoreDb.Create(Configuration["FirebaseProjectId"]);
            CommonService.Logger = logger;
        }   
    }
}