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
        public string NugetVersion { get; }

        public Component(string id, string version, string nugetOrgVersion, string path, Project project)
        {
            Id = id;
            Version = version;
            Path = path;
            Project = project;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            NugetVersion = nugetOrgVersion;
        }
    }
}
