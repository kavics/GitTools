using GitT.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitT
{
    internal class Program
    {
        private static readonly string[] CommandNames = new string[] {"components", "configure", "status"};
        private static readonly IHost Host = CreateHost();

        private static void Main(string[] args)
        {
//args = new[] { "components", "-refs" };
args = new[] { "components", "-nuget" };

            var githubContainer = Directory.GetCurrentDirectory();
githubContainer = @"D:\dev\github";

            Run(githubContainer, args);
        }

        private static IHost CreateHost()
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddSingleton<INugetTools, NugetTools>()
                        .AddKeyedTransient<ICommand, ComponentsCommand>("components")
                        .AddKeyedTransient<ICommand, ConfigureCommand>("configure")
                        .AddKeyedTransient<ICommand, StatusCommand>("status")
                        ;

                }).Build();
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
}
