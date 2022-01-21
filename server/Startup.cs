using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
using SolidTradeServer.Services.Cache;
using SolidTradeServer.Services.Jobs;
using SolidTradeServer.Services.TradeRepublic;
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
            
            services.AddDbContext<DbSolidTrade>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddSingleton<OngoingProductsService>();
            services.AddSingleton<CloudinaryService>();
            services.AddSingleton<TradeRepublicApiService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<RemoveKnockedOutProductsJobsService>();
            services.AddSingleton<RemoveOngoingExpiredTradeJobsService>();
            services.AddSingleton<RemoveExpiredWarrantProductsJobsService>();

            services.AddTransient<UserService>();
            services.AddTransient<StockService>();
            services.AddTransient<WarrantService>();
            services.AddTransient<KnockoutService>();
            services.AddTransient<PortfolioService>();
            services.AddTransient<NotificationService>();
            services.AddTransient<OngoingWarrantService>();
            services.AddTransient<AuthenticationService>();
            services.AddTransient<OngoingKnockoutService>();
            services.AddTransient<HistoricalPositionsService>();
            
            services.AddLogging();

            services.AddControllers(options =>
            {
                options.Filters.Add<AuthenticationFilter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.Converters.Add(new StringRemoveWhitespaceConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(null, false));
            });

            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage());

            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, ILogger logger)
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

            var filter = new BasicAuthAuthorizationFilter(
                new BasicAuthAuthorizationFilterOptions
                {
                    // Require secure connection for dashboard
                    RequireSsl = true,
                    // Case sensitive login checking
                    LoginCaseSensitive = true,
                    Users = new[]
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = Configuration["Hangfire:User"],
                            PasswordClear = Configuration["Hangfire:Password"],
                        },
                    }
                });
            
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new [] { filter }
            });

            var removeOngoingExpiredTradeJobsService = serviceProvider.GetService<RemoveOngoingExpiredTradeJobsService>();

            if (removeOngoingExpiredTradeJobsService is null)
                throw new Exception("The service RemoveOngoingExpiredTradesService could not be provided.");

            var removeKnockedOutProductsJobsService = serviceProvider.GetService<RemoveKnockedOutProductsJobsService>();

            if (removeKnockedOutProductsJobsService is null)
                throw new Exception("The service RemoveOngoingExpiredTradesService could not be provided.");

            var removeExpiredWarrantProductsJobsService = serviceProvider.GetService<RemoveExpiredWarrantProductsJobsService>();

            if (removeExpiredWarrantProductsJobsService is null)
                throw new Exception("The service RemoveExpiredWarrantProductsJobsService could not be provided.");
            
            recurringJobManager.AddOrUpdate("Remove Ongoing expired trades", () => removeOngoingExpiredTradeJobsService.StartAsync(), Cron.Daily);
            recurringJobManager.AddOrUpdate("Remove Expired warrants", () => removeExpiredWarrantProductsJobsService.StartAsync(), Cron.Weekly(DayOfWeek.Sunday));
            recurringJobManager.AddOrUpdate("Remove Knocked out products", () => removeKnockedOutProductsJobsService.StartAsync(), Cron.Weekly(DayOfWeek.Sunday));
            
            // Insures the trade republic service is being instantiated at the beginning of the application.
            app.ApplicationServices.GetService<TradeRepublicApiService>();
            
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(Configuration["FirebaseCredentials"]),
            });
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Configuration["FirebaseCredentials"]);
            OngoingProductsService.Firestore = FirestoreDb.Create(Configuration["FirebaseProjectId"]);
        }
    }
}