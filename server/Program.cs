using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using Serilog.Events;
using SolidTradeServer.Serilog;

namespace SolidTradeServer
{
    public static class Program
    {
        public static void Main(string[] args)
           => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.AddJsonFile("config/appsettings.json");
                    hostConfig.AddJsonFile("config/appsettings.credentials.json");
                    hostConfig.AddEnvironmentVariables();
                })
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .MinimumLevel.Verbose()
                        .Enrich.With(new SerilogMessageEnricher())
                        .WriteTo.Console(
                            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}",
                            restrictedToMinimumLevel: LogEventLevel.Information)
                        .WriteTo.Elasticsearch(
                            new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfiguration:Uri"]))
                            {
                                ModifyConnectionSettings = x =>
                                    x.BasicAuthentication(context.Configuration["ElasticConfiguration:Username"], 
                                        context.Configuration["ElasticConfiguration:Password"]),
                                IndexFormat =
                                    $"{context.Configuration["ElasticConfiguration:ApplicationName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace('.', '-')}-{DateTime.UtcNow:MM-yyyy}",
                                AutoRegisterTemplate = true,
                                NumberOfShards = 2,
                                NumberOfReplicas = 1,
                                MinimumLogEventLevel = LogEventLevel.Debug,
                            })
                        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                        .ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public static void ExitApplication() => Environment.Exit(-1);
    }
}