using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitT
{
    public enum CommitStatus { Default, Local, Behind, Ahead };

    public class Repo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Branch { get; set; }
        public CommitStatus CommitStatus { get; set; }
        public string Status { get; set; }
        public DateTime Modified { get; set; }
    }
}
