using System;

namespace Kavics.GittLib.Models
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

        //public static IEnumerable<RepositoryInfo> GetRepositories(string path, bool fetch, IGitTools gitTools)
        //{
        //    const string gitArgs = @"status -b -s";

        //    var repositories = Directory.GetDirectories(path)
        //        .Select(r =>
        //        {
        //            var name = System.IO.Path.GetFileName(r);
        //            if (fetch)
        //            {
        //                Console.Write("{0,-40}{1}", name, "fetching...\r");
        //                gitTools.Git(r, "fetch", out _, out _);
        //            }
        //            var gitOut = gitTools.Git(r, gitArgs, out _, out _);
        //            var repo = new RepositoryInfo { Path = r, Name = name, IsGithub = true };
        //            ParseStatus(repo, gitOut);
        //            repo.Modified = new DateTime(Math.Max(repo.Modified.Ticks, GetLastFetchDate(r).Ticks));
        //            return repo;
        //        });
        //    return repositories;
        //}
        //private static void ParseStatus(RepositoryInfo repo, string gitOut)
        //{
        //    var lines = gitOut.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    if (lines.Length == 0)
        //    {
        //        repo.IsGithub = false;
        //        return;
        //    }
        //    // parse branch (example: "## master...origin/master")
        //    var branchSrc = lines[0].Substring(3).Split(new[] { "...", " " }, StringSplitOptions.RemoveEmptyEntries);
        //    var branch = branchSrc[0];
        //    var commitStatus = CommitStatus.Default;
        //    var commitStatusText = string.Empty;
        //    if (branchSrc.Length == 1)
        //    {
        //        commitStatus = CommitStatus.Local;
        //        commitStatusText = "LOCAL";
        //    }
        //    else if (branchSrc.Length > 2)
        //    {
        //        commitStatus = branchSrc[2] == "[ahead" ? CommitStatus.Ahead : CommitStatus.Behind;
        //        commitStatusText = string.Join(" ", branchSrc.Skip(2).ToArray());
        //    }

        //    // parse lines of file statuses (example: " M src/Tests/....cs")
        //    var additions = 0;
        //    var modifications = 0;
        //    var deletions = 0;
        //    var others = 0;
        //    var modified = DateTime.MinValue;
        //    if (lines.Length > 1)
        //    {
        //        foreach (var line in lines.Skip(1))
        //        {
        //            string path;
        //            if (line.StartsWith("R  "))
        //            {
        //                var p = line.IndexOf(" -> ", StringComparison.Ordinal);
        //                if (p < 0)
        //                    throw new InvalidOperationException("Line is not parsed: " + line);
        //                path = System.IO.Path.Combine(repo.Path, line.Substring(p + 4));
        //            }
        //            else
        //            {
        //                path = System.IO.Path.Combine(repo.Path, line.Substring(3));
        //            }
        //            var time = DateTime.MinValue;
        //            if (Directory.Exists(path))
        //                time = Directory.GetLastWriteTime(path);
        //            if (File.Exists(path))
        //                time = File.GetLastWriteTime(path);
        //            if (time > modified)
        //                modified = time;

        //            switch (line.TrimStart()[0])
        //            {
        //                case 'A': additions++; break;
        //                case 'M': modifications++; break;
        //                case 'D': deletions++; break;
        //                default: others++; break;
        //            }
        //        }
        //    }
        //    var status = additions + modifications + deletions + others == 0
        //        ? "" // "≡" 
        //        : $"+{additions} ~{modifications} -{deletions}";
        //    if (others != 0)
        //        status += $" ?{others}";
        //    status += (status.Length == 0 ? "" : " ") + commitStatusText;

        //    repo.Branch = branch;
        //    repo.CommitStatus = commitStatus;
        //    repo.Status = status;
        //    repo.Modified = modified;
        //}

        //private static DateTime GetLastFetchDate(string path)
        //{
        //    var filePath = System.IO.Path.Combine(path, @".git\HEAD");
        //    var result = File.GetLastWriteTime(filePath);

        //    filePath = System.IO.Path.Combine(path, @".git\FETCH_HEAD");
        //    if (File.Exists(filePath))
        //        result = Max(result, File.GetLastWriteTime(filePath));

        //    filePath = System.IO.Path.Combine(path, @".git\COMMIT_EDITMSG");
        //    if (File.Exists(filePath))
        //        result = Max(result, File.GetLastWriteTime(filePath));

        //    return result;
        //}
        //private static DateTime Max(DateTime value1, DateTime value2)
        //{
        //    return value2 > value1 ? value2 : value1;
        //}

    }
}
