using GitT.Models;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Common;
using NuGet.Protocol;
using Repository = NuGet.Protocol.Core.Types.Repository;

namespace GitT
{
    public interface INugetTools
    {
        Task<PublishedVersion> GetLatestVersionAsync(string packageId, CancellationToken cancel);
    }

    public class NugetTools : INugetTools
    {
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly SourceCacheContext _cache = new SourceCacheContext();

        public async Task<PublishedVersion> GetLatestVersionAsync(string packageId, CancellationToken cancel)
        {

            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
            IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(packageId, _cache, _logger, cancel);

            var lastVersion = versions.LastOrDefault()?.OriginalVersion ?? string.Empty;
            if (lastVersion == string.Empty)
                return PublishedVersion.Empty;

            PackageMetadataResource resource2 = await repository.GetResourceAsync<PackageMetadataResource>(cancel);
            IEnumerable<IPackageSearchMetadata> packages = await resource2.GetMetadataAsync(packageId,
                true, false, _cache,_logger, cancel);
            var lastPackage = packages.FirstOrDefault(p => p.Identity.Version.OriginalVersion == lastVersion);
            var published = lastPackage?.Published ?? DateTimeOffset.MinValue;

            return new PublishedVersion {PublishedDate = published, Version = lastVersion};
        }
    }
}
