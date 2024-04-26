using System.Diagnostics;

namespace GitT.Models
{
    public class PublishedVersion
    {
        public DateTimeOffset PublishedDate { get; set; }
        public string Version { get; set; }

        public static readonly PublishedVersion Empty =
            new PublishedVersion{Version = string.Empty, PublishedDate = DateTimeOffset.MinValue};
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Component
    {
        public string Id { get; }
        public string Version { get; }
        public string Path { get; }
        public string Name { get; }
        public Project Project { get; }

        public PublishedVersion NugetVersion { get; }

        public Component(string id, string version, PublishedVersion nugetOrgVersion, string path, Project project)
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
