using GitT.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace GitT;

public class GitToolsOptions
{
    public string? GitHubToken { get; set; }
}

internal class Program
{
    private static readonly string[] CommandNames = new string[] {"components", "configure", "status", "repositories" };

    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(configBuilder =>
        {
            // these operations are necessary because the app is not always started from the current folder
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            configBuilder.AddJsonFile(file);
            configBuilder.AddUserSecrets("e42a77d8-2766-47dc-90ee-8ce0f0e63f67");
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddSingleton<IGitHubTools, GithubTools>()
                .AddSingleton<INugetTools, NugetTools>()
                .AddKeyedTransient<ICommand, ComponentsCommand>("components")
                .AddKeyedTransient<ICommand, ConfigureCommand>("configure")
                .AddKeyedTransient<ICommand, StatusCommand>("status")
                .AddKeyedTransient<ICommand, RepositoriesCommand>("repositories")
                .Configure<GitToolsOptions>(opt =>
                {
                    context.Configuration.GetSection("gitTools").Bind(opt);
                });

        }).Build();

    private static void Main(string[] args)
    {
        var githubContainer = Directory.GetCurrentDirectory();

        Run(githubContainer, args);
    }

    private static void Run(string githubContainer, string[] args)
    {
        var command = GetCommand(args.FirstOrDefault());
        if (command == null)
            return;

        try
        {
            var context = new CommandContext(githubContainer, command, args.Skip(1).ToArray());
            if (!(command is ConfigureCommand) && context.Config.GitExePath == null)
            {
                Console.WriteLine("GitT cannot run because git.exe was not found.");
                Console.WriteLine("Please configure the full path of the git.exe with the following command:");
                Console.WriteLine("GitT Configure GitExe <fullpath>");

                return;
            }
            command.Context = context;
            command.Execute();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static readonly string[] HelpStrings = { "/?", "/h", "/help", "-?", "-h", "-help", "--help" };
    internal static ICommand? GetCommand(string? commandName)
    {
        if (commandName == null)
            commandName = "Status";
        else if (HelpStrings.Contains(commandName.ToLowerInvariant()))
        {
            WriteHelp();
            return null;
        }

        var command = Host.Services.GetKeyedService<ICommand>(commandName.ToLowerInvariant());
        if (command != null)
            return command;

        WriteError($"Unknown command: {commandName}\r\n{GetAvailableCommandsMessage()}");
        return null;
    }

    private static void WriteError(string message)
    {
        Console.WriteLine(message);
    }

    private static string GetAvailableCommandsMessage() =>
        "Available commands:\r\n  " + string.Join("\r\n  ", CommandNames);

    private static void WriteHelp()
    {

        var message = "GitT </? | -? | /h | -h | /help | -help | --help>\r\n" +
                      "GitT <command> [command-arguments]\r\n" +
                      "GitT <command> </? | -? | /h | -h | /help | -help | --help>\r\n" +
                      "\r\n" + GetAvailableCommandsMessage();

        Usage(message);

    }

    private static void Usage(string message)
    {
        Console.WriteLine();
        Console.WriteLine("Git Tools V0.2");
        Console.WriteLine("==============");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine(message);
        Console.WriteLine();
    }
}