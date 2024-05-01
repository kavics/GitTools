using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kavics.GittLib
{
    public interface IGitTools
    {
        string Git(string repoPath, string gitArgs, out int exitCode, out string stdErr);
    }

    public class GitTools : IGitTools
    {
        private UserSettings _userSettings;

        public GitTools(IOptions<UserSettings> userSettings)
        {
            _userSettings = userSettings?.Value ?? new UserSettings();
        }
        public string Git(string repoPath, string gitArgs, out int exitCode, out string stdErr)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = _userSettings.GitExePath,
                Arguments = gitArgs,
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });

            if (process == null)
            {
                exitCode = int.MinValue;
                stdErr = string.Empty;
                return string.Empty;
            }

            stdErr = process.StandardError.ReadToEnd();
            var stdOut = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            exitCode = process.ExitCode;
            process.Close();

            return stdOut;
        }
    }
}
