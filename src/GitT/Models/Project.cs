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
        public List<Component> Components { get; } = new List<Component>();
        public List<Package> Packages { get; } = new List<Package>();

        public Project(Repository repository, string prjPath)
        {
            Repository = repository;
            PrjPath = prjPath;
            Path = System.IO.Path.GetDirectoryName(prjPath);
            Name = System.IO.Path.GetFileNameWithoutExtension(prjPath);
        }
    }
}
