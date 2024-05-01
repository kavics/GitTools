using Kavics.GittLib;
using Kavics.GittLib.Controllers;
using Kavics.GittLib.Models;
using Microsoft.Extensions.DependencyInjection;
using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    // ReSharper disable once UnusedMember.Global
    public class StatusCommand : ICommand
    {
        public string ShortInfo => "Shows current branch, Status and last fetch date of every repository.";
        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private StatusArguments _args;
        public void Execute()
        {
            try
            {
                if (!Context.ParseArguments(out _args))
                    return;
            }
            catch (ParsingException e)
            {
                Console.WriteLine(e.FormattedMessage);
                return;
            }

            var fetch = _args.Fetch;

            InitializeColors();

            CurrentBranch(Context.GithubContainer, fetch);
        }
        private void CurrentBranch(string path, bool fetch)
        {
            Console.WriteLine("REPOSITORIES");
            Console.WriteLine("{0,-40}{1,-30}{2,-25}{3}", "Repository", "Current branch", "Status", "Modified/Last Fetch");
            Console.WriteLine("======================================= ============================= ======================== ===================");

            var controller = Context.Services.GetRequiredService<ILocalRepositoryController>();
            var repositories = controller.GetRepositories(path, fetch);

            foreach (var repo in repositories)
            {
                if (repo.IsGithub)
                {
                    Console.Write("{0,-40}", repo.Name);
                    using (BranchColor(repo.Branch))
                        Console.Write("{0,-30}", repo.Branch);
                    using (StatusColor(repo.CommitStatus))
                        Console.Write("{0,-25}", repo.Status);
                    SetDefaultColor();
                    Console.WriteLine(DateTools.FormatDate(repo.Modified));
                }
                else
                {
                    using (new ColoredBlock(ConsoleColor.White, ConsoleColor.DarkRed))
                    {
                        Console.Write("{0,-40}", repo.Name);
                        Console.Write("{0,-30}", "Not a github repository");
                        Console.Write("{0,-25}", "");
                        Console.Write("{0,-19}", "");
                        Console.WriteLine();
                    }
                }
            }
        }

        /* ========================================================================= Color support */

        private static ConsoleColor _defaultBackgroundColor;
        private static ConsoleColor _defaultForegroundColor;

        private static void InitializeColors()
        {
            _defaultBackgroundColor = Console.BackgroundColor;
            _defaultForegroundColor = Console.ForegroundColor;
        }

        private static void SetDefaultColor()
        {
            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
        }
        private static IDisposable BranchColor(string branch)
        {
            switch (branch)
            {
                case "master":
                    return new ColoredBlock(ConsoleColor.Cyan, _defaultBackgroundColor);
                case "develop":
                    return new ColoredBlock(_defaultForegroundColor, _defaultBackgroundColor);
                default:
                    return new ColoredBlock(ConsoleColor.Yellow, _defaultBackgroundColor);
            }
        }
        private static IDisposable StatusColor(CommitStatus commitStatus)
        {
            switch (commitStatus)
            {
                case CommitStatus.Default:
                    return new ColoredBlock(ConsoleColor.Green, _defaultBackgroundColor);
                case CommitStatus.Local:
                    return new ColoredBlock(ConsoleColor.Red, _defaultBackgroundColor);
                case CommitStatus.Behind:
                    return new ColoredBlock(ConsoleColor.Yellow, _defaultBackgroundColor);
                case CommitStatus.Ahead:
                    return new ColoredBlock(ConsoleColor.Green, _defaultBackgroundColor);
                default:
                    throw new ArgumentOutOfRangeException(nameof(commitStatus), commitStatus, null);
            }
        }

        private class ColoredBlock : IDisposable
        {
            public ColoredBlock(ConsoleColor foreground, ConsoleColor background)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
            }
            public void Dispose()
            {
                SetDefaultColor();
            }
        }
        //private class NoColor : IDisposable
        //{
        //    public static readonly NoColor Instance = new NoColor();
        //    public void Dispose() { }
        //}

    }
}
