using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Database;
using Infrastructure.Extensions;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using WebAPI.Serilog;
using ILogger = Serilog.ILogger;

namespace WebAPI;

public static class Program
{
    private const string SerilogOutputTemplate =
        "{Timestamp:yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}";

    private static readonly ILogger Logger = Log.ForContext(typeof(Program));

    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args)
            .Build()
            .MigrateDatabase<IApplicationDbContext>();

        SelfLog.Enable(Logger.Error);

        host.Run();
    }

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
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.With(new SerilogMessageEnricher())
                    .WriteTo.Map(_ => DateTimeOffset.Now,
                        (v, wt) =>
                            wt.File($"/var/log/solidtrade/api/{v:MM-yyyy}/log-{v:dd-MM-yyyy ddd zz} - .log",
                                outputTemplate: SerilogOutputTemplate,
                                fileSizeLimitBytes: 2000000,
                                rollingInterval: RollingInterval.Day,
                                rollOnFileSizeLimit: true))
                    .WriteTo.Console(
                        outputTemplate: SerilogOutputTemplate,
                        restrictedToMinimumLevel: LogEventLevel.Information)
                    .WriteTo.Conditional(_ => context.Configuration.GetValue<bool>("ElasticConfiguration:Enable"), c =>
                        c.Elasticsearch(
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
                                MinimumLogEventLevel = LogEventLevel.Information,
                            }))
                    .ReadFrom.Configuration(context.Configuration);
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}