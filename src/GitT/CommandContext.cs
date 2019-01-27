using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Tools.CommandLineArguments;

namespace GitT
{
    public class CommandContext
    {
        private static string _gitWorkerExePath;

        public string GithubContainer { get; }
        public string[] Args { get; }
        internal ICommand Command { get; }

        internal CommandContext(string githubContainer, string gitWorkerExePath, ICommand command, string[] args)
        {
            GithubContainer = githubContainer;
            _gitWorkerExePath = gitWorkerExePath;
            Command = command;
            Args = args;
        }

        public bool ParseArguments<T>(out T arguments) where T : class, new()
        {
            arguments = new T();

            var result = ArgumentParser.Parse(Args, arguments);

            if (!result.IsHelp)
                return true;

            Console.WriteLine(InsertCommandName(result.GetHelpText(), Command.GetType().Name));
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

        public string Git(string repoPath, string gitArgs, out int exitCode, out string stdErr)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = _gitWorkerExePath,
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
