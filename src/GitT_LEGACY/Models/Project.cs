using System.Collections.Generic;
using System.Diagnostics;

namespace GitT.Models
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Project
    {
        public Repository Repository { get; }
        public string PrjPath { get; }
        public string Path { get; }
        public string Name { get; }
        public string Version { get; set; } = "0.0.0.0 NOT SET";
        public List<Component> Components { get; } = new List<Component>();
        public List<Package> Packages { get; } = new List<Package>();
        public List<string> ProjectReferences { get; } = new List<string>();
        public List<Project> Dependencies { get; } = new List<Project>();

        public Project(Repository repository, string prjPath)
        {
            Repository = repository;
            PrjPath = prjPath;
            Path = System.IO.Path.GetDirectoryName(prjPath);
            Name = System.IO.Path.GetFileNameWithoutExtension(prjPath);
        }
    }
}
