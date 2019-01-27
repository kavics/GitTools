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
    internal  enum CommitStatus { Default, Local, Behind, Ahead };
    internal class Repo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Branch { get; set; }
        public CommitStatus CommitStatus { get; set; }
        public string Status { get; set; }
        public DateTime Modified { get; set; }
    }
    internal class Program
    {
        private static string _gitWworkerExePath = @"C:\Program Files\Git\bin\git.exe";
        private static string _githubContainer = @"D:\dev\github";

        static void Main(string[] args)
        {
            //args = new[] {"-f"};
            //Run(args);

            var command =  GetCommand(args.FirstOrDefault());
            if (command == null)
                return;

            try
            {
                var context = new CommandContext(_githubContainer, _gitWworkerExePath,
                    args.Skip(1).ToArray());
                command.Execute(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                Console.WriteLine();
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
                CommandTypes.FirstOrDefault(t => t.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
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
            return "Available commands:\r\n" + string.Join("\r\n", CommandTypes.Select(t => t.Name));
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
            Console.WriteLine("Git Tools V0.1");
            Console.WriteLine("==============");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(message);
            Console.WriteLine();
        }



    }
}
