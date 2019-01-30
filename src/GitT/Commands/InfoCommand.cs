using System;
using System.IO;
using System.Linq;
using GitT.Models;

namespace GitT.Commands
{
    public class InfoCommand : ICommand
    {
        public string ShortInfo => "Shows current branch, Status and last fetch date of every repository.";
        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private InfoArguments _args;
        public void Execute()
        {
            if (!Context.ParseArguments<InfoArguments>(out _args))
                return;

            var fetch = _args.Fetch;

            InitializeColors();

            CurrentBranch2(Context.GithubContainer, fetch);
        }
        private void CurrentBranch1(string path)
        {
            var gitArgs = @"rev-parse --abbrev-ref HEAD";

            Console.WriteLine("Repositories");
            Console.WriteLine("{0,-30}{1,-30}{2}", "Repository", "Current branch", "Modified");
            Console.WriteLine("============================= ============================= ===================");

            var enumerable = Directory.GetDirectories(path)
                .Select(r => new
                {
                    name = Path.GetFileName(r),
                    branch = Context.Git(r, gitArgs, out _, out _).Trim(),
                    modified = GetLastFetchDate(r).ToString("yyyy-MM-dd HH:mm:ss")
                });

            foreach (var repo in enumerable)
            {
                Console.Write("{0,-30}", repo.name);
                using (BranchColor(repo.branch))
                    Console.Write("{0,-30}", repo.branch);
                SetDefaultColor();
                Console.WriteLine(repo.modified);
            }
        }
        private void CurrentBranch2(string path, bool fetch)
        {
            var gitArgs = @"status -b -s";

            Console.WriteLine("REPOSITORIES");
            Console.WriteLine("{0,-40}{1,-30}{2,-25}{3}", "Repository", "Current branch", "Status", "Modified/Last Fetch");
            Console.WriteLine("======================================= ============================= ======================== ===================");

            var enumerable = Directory.GetDirectories(path)
                .Select(r =>
                {
                    var name = Path.GetFileName(r);
                    if (fetch)
                    {
                        Console.Write("{0,-40}{1}", name, "fetching...\r");
                        Context.Git(r, "fetch", out _, out _);
                    }
                    var gitOut = Context.Git(r, gitArgs, out _, out _);
                    var repo = new RepositoryInfo { Path = r, Name = name };
                    ParseStatus(repo, gitOut);
                    repo.Modified = new DateTime(Math.Max(repo.Modified.Ticks, GetLastFetchDate(r).Ticks));
                    return repo;
                });

            foreach (var repo in enumerable)
            {
                Console.Write("{0,-40}", repo.Name);
                using (BranchColor(repo.Branch))
                    Console.Write("{0,-30}", repo.Branch);
                using (StatusColor(repo.CommitStatus))
                    Console.Write("{0,-25}", repo.Status);
                SetDefaultColor();
                Console.WriteLine(DateTools.FormatDate(repo.Modified));
            }
        }
        private void ParseStatus(RepositoryInfo repo, string gitOut)
        {
            var lines = gitOut.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // parse branch (example: "## master...origin/master")
            var branchSrc = lines[0].Substring(3).Split(new[] { "...", " " }, StringSplitOptions.RemoveEmptyEntries);
            var branch = branchSrc[0];
            var commitStatus = CommitStatus.Default;
            var commitStatusText = string.Empty;
            if (branchSrc.Length == 1)
            {
                commitStatus = CommitStatus.Local;
                commitStatusText = "LOCAL";
            }
            else if (branchSrc.Length > 2)
            {
                commitStatus = branchSrc[2] == "[ahead" ? CommitStatus.Ahead : CommitStatus.Behind;
                commitStatusText = string.Join(" ", branchSrc.Skip(2).ToArray());
            }

            // parse lines of file statuses (example: " M src/Tests/....cs")
            var additions = 0;
            var modifications = 0;
            var deletions = 0;
            var others = 0;
            var modified = DateTime.MinValue;
            if (lines.Length > 1)
            {
                foreach (var line in lines.Skip(1))
                {
                    var time = DateTime.MinValue;
                    var path = Path.Combine(repo.Path, line.Substring(3));
                    if (Directory.Exists(path))
                        time = Directory.GetLastWriteTime(path);
                    if (File.Exists(path))
                        time = File.GetLastWriteTime(path);
                    if (time > modified)
                        modified = time;

                    switch (line.TrimStart()[0])
                    {
                        case 'A': additions++; break;
                        case 'M': modifications++; break;
                        case 'D': deletions++; break;
                        default: others++; break;
                    }
                }
            }
            var status = (additions + modifications + deletions + others == 0)
                ? "" // "≡" 
                : $"+{additions} ~{modifications} -{deletions}";
            if (others != 0)
                status += $" ?{others}";
            status += (status.Length == 0 ? "" : " ") + commitStatusText;

            repo.Branch = branch;
            repo.CommitStatus = commitStatus;
            repo.Status = status;
            repo.Modified = modified;
        }

        private static DateTime GetLastFetchDate(string path)
        {
            var filePath = Path.Combine(path, @".git\HEAD");
            var result = File.GetLastWriteTime(filePath);

            filePath = Path.Combine(path, @".git\FETCH_HEAD");
            if (File.Exists(filePath))
                result = Max(result, File.GetLastWriteTime(filePath));

            filePath = Path.Combine(path, @".git\COMMIT_EDITMSG");
            if (File.Exists(filePath))
                result = Max(result, File.GetLastWriteTime(filePath));

            return result;
        }
        private static DateTime Max(DateTime value1, DateTime value2)
        {
            return value2 > value1 ? value2 : value1;
        }

        /* ========================================================================= Color support */

        private static ConsoleColor DefaultBackgroundColor;
        private static ConsoleColor DefaultForegroundColor;

        private static void InitializeColors()
        {
            DefaultBackgroundColor = Console.BackgroundColor;
            DefaultForegroundColor = Console.ForegroundColor;
        }

        private static void SetDefaultColor()
        {
            Console.BackgroundColor = DefaultBackgroundColor;
            Console.ForegroundColor = DefaultForegroundColor;
        }
        private static IDisposable BranchColor(string branch)
        {
            switch (branch)
            {
                case "master":
                    return new ColoredBlock(ConsoleColor.Cyan, DefaultBackgroundColor);
                case "develop":
                    return new ColoredBlock(DefaultForegroundColor, DefaultBackgroundColor);
                default:
                    return new ColoredBlock(ConsoleColor.Yellow, DefaultBackgroundColor);
            }
        }
        private static IDisposable StatusColor(CommitStatus commitStatus)
        {
            switch (commitStatus)
            {
                case CommitStatus.Default:
                    return new ColoredBlock(ConsoleColor.Green, DefaultBackgroundColor);
                case CommitStatus.Local:
                    return new ColoredBlock(ConsoleColor.Red, DefaultBackgroundColor);
                case CommitStatus.Behind:
                    return new ColoredBlock(ConsoleColor.Yellow, DefaultBackgroundColor);
                case CommitStatus.Ahead:
                    return new ColoredBlock(ConsoleColor.Green, DefaultBackgroundColor);
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
        private class NoColor : IDisposable
        {
            public static readonly NoColor Instance = new NoColor();
            public void Dispose() { }
        }

    }
}
