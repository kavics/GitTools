using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GitT.Commands;
using SenseNet.Tools;

namespace GitT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Directory.SetCurrentDirectory(@"D:\Projects\github\kavics");
            //args = new[] { "components", "-r" };

            var githubContainer = Directory.GetCurrentDirectory();

            Run(githubContainer, args);

            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        static void Run(string githubContainer, string[] args)
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

        private static readonly string[] HelpStrings = {"/?", "/h", "/help", "-?", "-h", "-help", "--help"};
        internal static ICommand GetCommand(string commandName)
        {
            if (commandName == null)
            {
                WriteHelp();
                return null;
            }
            if(HelpStrings.Contains(commandName.ToLowerInvariant()))
            {
                WriteHelp();
                return null;
            }

            var commandType =
                CommandTypes.FirstOrDefault(t =>
                    t.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase) ||
                    t.Name.Equals(commandName + "command", StringComparison.OrdinalIgnoreCase));
            if (commandType != null)
                return (ICommand)Activator.CreateInstance(commandType);

            WriteError($"Unknown command: {commandName}\r\n{GetAvailableCommandsMessage()}");
            return null;
        }

        private static void WriteError(string message)
        {
            Console.WriteLine(message);
        }

        private static string GetAvailableCommandsMessage()
        {
            return "Available commands:\r\n" + string.Join("\r\n",
                CommandTypes.Select(t => "  " + CommandContext.GetCommandName(t)).OrderBy(n => n));
        }

        private static readonly Type[] CommandTypes = new Lazy<Type[]>(() =>
            TypeResolver.GetTypesByInterface(typeof(ICommand))).Value;

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
