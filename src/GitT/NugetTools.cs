using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Common;
using NuGet.Protocol;

namespace GitT
{
    public interface INugetTools
    {
        Task<string?> GetLatestVersionAsync(string packageId, CancellationToken cancel);
    }

    public class NugetTools : INugetTools
    {
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly SourceCacheContext _cache = new SourceCacheContext();

        public async Task<string?> GetLatestVersionAsync(string packageId, CancellationToken cancel)
        {

            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
            IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(packageId, _cache, _logger, cancel);

            //return versions.Max(p => p.OriginalVersion ?? string.Empty);
            return versions.LastOrDefault()?.OriginalVersion ?? string.Empty;
        }
    }
}
