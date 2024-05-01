namespace Kavics.GittLib
{
    //UNDONE: UserSettings
    public class UserSettings
    {
        // ReSharper disable once ConvertToConstant.Local
        private static readonly string GitWorkerExeDefaultPath = @"C:\Program Files\Git\bin\git.exe";

        public string? GitExePath { get; set; } = @"C:\Program Files\Git\bin\git.exe";
        public string? InternalNugetPath { get; set; }
        public string? PrivateNugetPath { get; set; }


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
