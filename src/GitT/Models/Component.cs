using System.Diagnostics;
using System.Linq;
using NuGet;

namespace GitT.Models
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Component
    {
        public string Id { get; }
        public string Version { get; }
        public string Path { get; }
        public string Name { get; }
        public Project Project { get; }

        private string _nugetVersion;
        public string NugetVersion
        {
            get
            {
return null;

                if (_nugetVersion == null)
                {
                    var repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
                    var packages = repo.FindPackagesById(Id).ToArray();
                    _nugetVersion = packages.Any()
                        ? packages.Max(p => p.Version).ToString()
                        : string.Empty;
                }
                return _nugetVersion;
            }
        }

        public Component(string id, string version, string path, Project project)
        {
            Id = id;
            Version = version;
            Path = path;
            Project = project;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
