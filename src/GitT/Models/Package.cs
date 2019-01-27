using System.Diagnostics;

namespace GitT.Models
{
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Package
    {
        public string Id { get; }
        public string Version { get; }
        public string TargetFramework { get; }
        public Project Project { get; set; }

        public Package(string id, string version, string targetFramework, Project project)
        {
            Id = id;
            Version = version;
            TargetFramework = targetFramework;
            Project = project;
        }
    }
}
