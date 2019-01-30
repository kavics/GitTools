using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using SenseNet.Tools.CommandLineArguments;

namespace GitT
{
    public class CommandContext
    {
        public UserSettings Config { get; }

        public string GithubContainer { get; }
        public string[] Args { get; }
        public ICommand Command { get; }

        internal CommandContext(string githubContainer, ICommand command, string[] args)
        {
            Config = UserSettings.Load();
            GithubContainer = githubContainer;
            Command = command;
            Args = args;
        }

        public bool ParseArguments<T>(out T arguments) where T : class, new()
        {
            arguments = new T();

            var result = ArgumentParser.Parse(Args, arguments);

            if (!result.IsHelp)
                return true;

            Console.WriteLine(InsertCommandName(result.GetHelpText(), GetCommandName(Command.GetType())));
            arguments = null;
            return false;
        }

        private string InsertCommandName(string helpText, string name)
        {
            var p = helpText.IndexOf("Usage:", StringComparison.Ordinal);
            p = helpText.IndexOf("GitT", p, StringComparison.Ordinal) + 5;
            var before = helpText.Substring(0, p);
            var after = helpText.Substring(p);
            return before + name + " " + after;
        }

        /* ============================================================================= Tools */

        public static string GetCommandName(Type type)
        {
            var name = type.Name;
            return name.EndsWith("Command")
                ? name.Substring(0, name.Length - 7)
                : name;
        }

        public static string GetNugetOrgVersion(string packageId)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            var packages = repo.FindPackagesById(packageId).ToArray();
            return packages.Any()
                ? packages.Max(p => p.Version).ToString()
                : string.Empty;
        }

        public string Git(string repoPath, string gitArgs, out int exitCode, out string stdErr)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = Config.GitExePath,
                Arguments = gitArgs,
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            });

            stdErr = process.StandardError.ReadToEnd();
            var stdOut = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            exitCode = process.ExitCode;
            process.Close();

            return stdOut;
        }
    }
}
