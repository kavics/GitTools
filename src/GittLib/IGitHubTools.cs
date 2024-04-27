using Kavics.GittLib.Models;
using octo

namespace Kavics.GittLib;

public interface IGitHubTools
{
    Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string name, CancellationToken cancel);
    Task<IReadOnlyList<Branch>> GetBranchesForRepositoryAsync(long repositoryId, CancellationToken cancel);
    Task<IReadOnlyList<Issue>> GetIssuesForRepositoryAsync(long repositoryId, CancellationToken cancel);

}