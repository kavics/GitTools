using System.IO;

namespace GitT
{
    //UNDONE: UserSettings
    public class UserSettings
    {
        // ReSharper disable once ConvertToConstant.Local
        private static readonly string GitWorkerExeDefaultPath = @"C:\Program Files\Git\bin\git.exe";

        public string? GitExePath { get; private set; }
        public string? InternalNugetPath { get; private set; }
        public string? PrivateNugetPath { get; private set; }


        public static UserSettings Load()
        {
            //var settings = Properties.Settings.Default;

            //var gitExePath = settings.GitExe;
            //if (string.IsNullOrEmpty(gitExePath))
            //    gitExePath = GitWorkerExeDefaultPath;
            //return new UserSettings
            //{
            //    GitExePath = File.Exists(gitExePath) ? gitExePath : null,
            //    InternalNugetPath = settings.InternalNuget,
            //    PrivateNugetPath = settings.PrivateNuget
            //};
            return new UserSettings
            {
                GitExePath = GitWorkerExeDefaultPath,
                InternalNugetPath = string.Empty,
                PrivateNugetPath = string.Empty
            };
        }
    }
}
