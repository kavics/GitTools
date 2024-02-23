using SenseNet.Tools.CommandLineArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GitT.Commands
{
    internal class RepositoriesCommand : ICommand
    {
        private readonly IGitHubTools _gitHubTools;
        private RepositoriesArguments _args;

        public string ShortInfo => "Shows all repositories, branches, issues of an organization";
        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        public RepositoriesCommand(IGitHubTools gitHubTools)
        {
            _gitHubTools = gitHubTools;
        }

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

            if (_args.Branches)
                BranchesAsync(_args.OrganizationName, CancellationToken.None).GetAwaiter().GetResult();
            else if (_args.Issues)
                IssuesAsync(_args.OrganizationName, CancellationToken.None).GetAwaiter().GetResult();
            else
                RepositoriesAsync(_args.OrganizationName, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task RepositoriesAsync(string orgName, CancellationToken cancel)
        {
            Console.WriteLine($"All branches by repositories of " + orgName);
            Console.Write("getting repositories...\r");
            var repositories = await _gitHubTools.GetRepositoriesAsync(orgName, cancel).ConfigureAwait(false);
            Console.Write("                       \r");
            Console.WriteLine("ACTIVE");
            var issuesTotalCount = 0;
            var activeRepositoryCount = 0;
            var archivedRepositoryCount = 0;
            foreach (var repository in repositories.Where(r => !r.Archived).OrderBy(r => r.Name))
            {
                issuesTotalCount += repository.OpenIssuesCount;
                activeRepositoryCount++;

                var issues = repository.OpenIssuesCount > 0 ? $"issues: {repository.OpenIssuesCount}" : string.Empty;
                Console.WriteLine($"    {repository.Name,-50} {issues}");
            }
            Console.WriteLine();
            Console.WriteLine("ARCHIVE");
            foreach (var repository in repositories.Where(r => r.Archived).OrderBy(r => r.Name))
            {
                archivedRepositoryCount++;
                Console.WriteLine($"    {repository.Name,-50} {repository.Language}");
            }
            Console.WriteLine();
            Console.WriteLine("ISSUES:      " + issuesTotalCount);
            Console.WriteLine("REPOSITORIES");
            Console.WriteLine("     ACTIVE: " + activeRepositoryCount);
            Console.WriteLine("    ARCHIVE: " + archivedRepositoryCount);
        }

        private async Task BranchesAsync(string orgName, CancellationToken cancel)
        {
            Console.WriteLine($"All branches by repositories of " + orgName);
            Console.Write("getting repositories...\r");
            var repositories = await _gitHubTools.GetRepositoriesAsync(orgName, cancel).ConfigureAwait(false);
            Console.Write("                       \r");
            var issuesTotalCount = 0;
            var activeRepositoryCount = 0;
            foreach (var repository in repositories.Where(r => !r.Archived).OrderBy(r => r.Name))
            {
                issuesTotalCount += repository.OpenIssuesCount;
                activeRepositoryCount++;

                var issues = repository.OpenIssuesCount > 0 ? $"issues: {repository.OpenIssuesCount}" : string.Empty;
                Console.WriteLine($"{repository.Name,-50} {issues}");
                Console.Write("getting branches...\r");
                var branches = await _gitHubTools.GetBranchesForRepositoryAsync(repository.Id, cancel).ConfigureAwait(false);
                Console.Write("                   \r");
                foreach (var branch in branches)
                    Console.WriteLine($"    {branch.Name}");
            }
            Console.WriteLine();
            Console.WriteLine("ISSUES:       " + issuesTotalCount);
            Console.WriteLine("REPOSITORIES: " + activeRepositoryCount);
        }
        private async Task IssuesAsync(string orgName, CancellationToken cancel)
        {
            Console.WriteLine($"All issues by repositories of " + orgName);
            Console.Write("getting repositories...\r");
            var repositories = await _gitHubTools.GetRepositoriesAsync(orgName, cancel).ConfigureAwait(false);
            Console.Write("                       \r");
            var issuesTotalCount = 0;
            var activeRepositoryCount = 0;
            foreach (var repository in repositories.Where(r => !r.Archived).OrderBy(r => r.Name))
            {
                issuesTotalCount += repository.OpenIssuesCount;
                activeRepositoryCount++;

                var issueCount = repository.OpenIssuesCount > 0 ? $"issues: {repository.OpenIssuesCount}" : string.Empty;
                Console.WriteLine($"{repository.Name,-50} {issueCount}");
                Console.Write("getting issues...\r");
                var issues = await _gitHubTools.GetIssuesForRepositoryAsync(repository.Id, cancel);
                Console.Write("                 \r");
                foreach (var issue in issues)
                {
                    var labels = string.Join(" ", issue.Labels.Select(x => $"[{x.Name}]"));
                    Console.WriteLine($"{issue.Number,8} {labels} {issue.Assignee?.Login}");
                    Console.WriteLine($"         {issue.Title}");
                }
            }
            Console.WriteLine();
            Console.WriteLine("ISSUES:       " + issuesTotalCount);
            Console.WriteLine("REPOSITORIES: " + activeRepositoryCount);
        }

    }
}
