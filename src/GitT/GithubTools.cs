using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Octokit;

namespace GitT;

public interface IGitHubTools
{
    Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string name, CancellationToken cancel);
    Task<IReadOnlyList<Branch>> GetBranchesForRepositoryAsync(long repositoryId, CancellationToken cancel);
    Task<IReadOnlyList<Issue>> GetIssuesForRepositoryAsync(long repositoryId, CancellationToken cancel);

}

/// <summary>
/// Singleton
/// </summary>
public class GithubTools : IGitHubTools
{
    private readonly GitToolsOptions _options;

    public GithubTools(IOptions<GitToolsOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string name, CancellationToken cancel)
    {
        var client = GetGithubClient();
        //var org = await client.Organization.Get(name);
        //var usr = await client.User.Get(name);

        try
        {
            var sensenetMaster = await client.Repository.Commit.Get("sensenet", "sensenet", "7103ace6b89ed70dbf250369d6374f614483ad21");
            var sensenetDevelop = await client.Repository.Commit.Get("sensenet", "sensenet", "7904629a6feaf850c9cef28492e8698ace52b8c4");

            var sensenetMasterDate = sensenetMaster.Commit.Author.Date;
            var sensenetDevelopDate = sensenetDevelop.Commit.Author.Date;

            return await client.Repository.GetAllForOrg(name).ConfigureAwait(false);
        }
        catch
        {
            // do nothing
        }
        try
        {
            return await client.Repository.GetAllForUser(name).ConfigureAwait(false);
        }
        catch
        {
            // do nothing
        }

        throw new ArgumentException("Unknown user or organization: " + name);
    }

    public Task<IReadOnlyList<Branch>> GetBranchesForRepositoryAsync(long repositoryId, CancellationToken cancel)
    {
        var client = GetGithubClient();
        return client.Repository.Branch.GetAll(repositoryId);
    }

    public async Task<IReadOnlyList<Issue>> GetIssuesForRepositoryAsync(long repositoryId, CancellationToken cancel)
    {
        var client = GetGithubClient();
        var issues = await client.Issue.GetAllForRepository(repositoryId).ConfigureAwait(false);
        return issues;
    }

    private GitHubClient? _pinnedGitHubClient;
    private GitHubClient GetGithubClient()
    {
        if(_pinnedGitHubClient == null)
        {
            var header = new ProductHeaderValue("GitTools");
            var client = new GitHubClient(header)
            {
                Credentials = new Credentials(_options.GitHubToken)
            };
            _pinnedGitHubClient = client;
        }

        return _pinnedGitHubClient;
    }
}