using System;

namespace GitT.Models
{
    public enum CommitStatus { Default, Local, Behind, Ahead }

    public class RepositoryInfo
    {
        public string Name { get; set; }
        public bool IsGithub { get; set; }
        public string Path { get; set; }
        public string Branch { get; set; }
        public CommitStatus CommitStatus { get; set; }
        public string Status { get; set; }
        public DateTime Modified { get; set; }
    }
}
