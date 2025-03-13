using CodeMechanic.Shargs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

internal class Program
{
    static async Task Main(string[] args)
    {
        var arguments = new ArgsMap(args);
        bool debug = arguments.HasFlag("--debug");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                "./logs/hydrolizer.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true
            )
            .CreateLogger();

        await RunAsCli(arguments, logger);
    }

    static async Task RunAsCli(ArgsMap arguments, Logger logger)
    {
        var services = CreateServices(arguments, logger);
        Application app = services.GetRequiredService<Application>();
        await app.Run();
    }

    private static ServiceProvider CreateServices(ArgsMap arguments,
        Logger logger)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(arguments)
            .AddSingleton<Logger>(logger)
            .AddScoped<SlnGenerator>()
            .AddSingleton<Application>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}

public class Application
{
    private readonly Logger logger;

    private readonly SlnGenerator sln_gen;

    public Application(
        Logger logger,
        SlnGenerator sln_gen
    )
    {
        this.logger = logger;
        this.sln_gen = sln_gen;
    }

    public async Task Run()
    {
        await sln_gen.Run();
    }
}