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

        internal CommandContext(string githubContainer, string gitWorkerExePath, string[] args)
        {
            GithubContainer = githubContainer;
            _gitWorkerExePath = gitWorkerExePath;
            Args = args;
        }

        public bool ParseArguments<T>(out T arguments) where T : class, new()
        {
            arguments = new T();

            var result = ArgumentParser.Parse(Args, arguments);

            if (!result.IsHelp)
                return true;

            Console.WriteLine(result.GetHelpText());
            arguments = null;
            return false;
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
