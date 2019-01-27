using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitT.Commands
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Project
    {
        public string PrjPath { get; }
        public string Path { get; }
        public string Name { get; }
        public List<Component> Components { get; } = new List<Component>();
        public List<Package> Packages { get; } = new List<Package>();

        public Project(string prjPath)
        {
            PrjPath = prjPath;
            Path = System.IO.Path.GetDirectoryName(prjPath);
            Name = System.IO.Path.GetFileNameWithoutExtension(prjPath);
        }
    }
}
