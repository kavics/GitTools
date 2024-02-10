using System.Collections.Generic;
using System.Diagnostics;

namespace GitT.Models
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Repository
    {
        public string Path { get; }
        public string Name { get; }
        public List<Project> Projects { get; } = new List<Project>();

        public Repository(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }
    }
}
