using System;
using System.IO;
using System.Reflection;

namespace GitT
{
    public class UserSettings
    {
        private static readonly string GitWorkerExeDefaultPath = @"C:\Program Files\Git\bin\git.exe";

        public string GitExePath { get; private set; }
        public string InternalNugetPath { get; private set; }
        public string PrivateNugetPath { get; private set; }


        public static UserSettings Load()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase;
            var settings = Properties.Settings.Default;

            var gitExePath = settings.GitExe;
            if (string.IsNullOrEmpty(gitExePath))
                gitExePath = GitWorkerExeDefaultPath;
            return new UserSettings()
            {
                GitExePath = File.Exists(gitExePath) ? gitExePath : null,
                InternalNugetPath = settings.InternalNuget,
                PrivateNugetPath = settings.PrivateNuget
            };
            //Properties.Settings.Default.PrivateNugetPath = "asdf";
            //Properties.Settings.Default.Save();


            throw new NotImplementedException();
            return new UserSettings();
        }

        private static bool CheckGit(string gitExePath)
        {
            return File.Exists(gitExePath);
        }
    }
}
