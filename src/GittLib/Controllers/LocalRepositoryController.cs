using System.Configuration;
using Kavics.GittLib.Models;
using System.Xml;

namespace Kavics.GittLib.Controllers;

public interface ILocalRepositoryController
{
    IEnumerable<RepositoryInfo> GetRepositoriesForStatusCommand(string path, bool fetch);
    Repository[] DiscoverRepositories(string githubContainerPath, bool nuget, IProgress<string> progress);
}

public class LocalRepositoryController : ILocalRepositoryController
{
    private readonly IGitTools _gitTools;
    private readonly INugetTools _nugetTools;
    public LocalRepositoryController(IGitTools gitTools, INugetTools nugetTools)
    {
        _gitTools = gitTools;
        _nugetTools = nugetTools;
    }

    public IEnumerable<RepositoryInfo> GetRepositoriesForStatusCommand(string path, bool fetch)
    {
        const string gitArgs = @"status -b -s";

        var repositories = Directory.GetDirectories(path)
            .Select(r =>
            {
                var name = Path.GetFileName(r);
                if (fetch)
                {
                    Console.Write("{0,-40}{1}", name, "fetching...\r");
                    _gitTools.Git(r, "fetch", out _, out _);
                }
                var gitOut = _gitTools.Git(r, gitArgs, out _, out _);
                var repo = new RepositoryInfo { Path = r, Name = name, IsGithub = true };
                ParseStatus(repo, gitOut);
                repo.Modified = new DateTime(Math.Max(repo.Modified.Ticks, GetLastFetchDate(r).Ticks));
                return repo;
            });
        return repositories;
    }
    private static void ParseStatus(RepositoryInfo repo, string gitOut)
    {
        var lines = gitOut.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            repo.IsGithub = false;
            return;
        }
        // parse branch (example: "## master...origin/master")
        var branchSrc = lines[0].Substring(3).Split(new[] { "...", " " }, StringSplitOptions.RemoveEmptyEntries);
        var branch = branchSrc[0];
        var commitStatus = CommitStatus.Default;
        var commitStatusText = string.Empty;
        if (branchSrc.Length == 1)
        {
            commitStatus = CommitStatus.Local;
            commitStatusText = "LOCAL";
        }
        else if (branchSrc.Length > 2)
        {
            commitStatus = branchSrc[2] == "[ahead" ? CommitStatus.Ahead : CommitStatus.Behind;
            commitStatusText = string.Join(" ", branchSrc.Skip(2).ToArray());
        }

        // parse lines of file statuses (example: " M src/Tests/....cs")
        var additions = 0;
        var modifications = 0;
        var deletions = 0;
        var others = 0;
        var modified = DateTime.MinValue;
        if (lines.Length > 1)
        {
            foreach (var line in lines.Skip(1))
            {
                string path;
                if (line.StartsWith("R  "))
                {
                    var p = line.IndexOf(" -> ", StringComparison.Ordinal);
                    if (p < 0)
                        throw new InvalidOperationException("Line is not parsed: " + line);
                    path = Path.Combine(repo.Path, line.Substring(p + 4));
                }
                else
                {
                    path = Path.Combine(repo.Path, line.Substring(3));
                }
                var time = DateTime.MinValue;
                if (Directory.Exists(path))
                    time = Directory.GetLastWriteTime(path);
                if (File.Exists(path))
                    time = File.GetLastWriteTime(path);
                if (time > modified)
                    modified = time;

                switch (line.TrimStart()[0])
                {
                    case 'A': additions++; break;
                    case 'M': modifications++; break;
                    case 'D': deletions++; break;
                    default: others++; break;
                }
            }
        }
        var status = additions + modifications + deletions + others == 0
            ? "" // "≡" 
            : $"+{additions} ~{modifications} -{deletions}";
        if (others != 0)
            status += $" ?{others}";
        status += (status.Length == 0 ? "" : " ") + commitStatusText;

        repo.Branch = branch;
        repo.CommitStatus = commitStatus;
        repo.Status = status;
        repo.Modified = modified;
    }
    private static DateTime GetLastFetchDate(string path)
    {
        var filePath = Path.Combine(path, @".git\HEAD");
        var result = File.GetLastWriteTime(filePath);

        filePath = Path.Combine(path, @".git\FETCH_HEAD");
        if (File.Exists(filePath))
            result = Max(result, File.GetLastWriteTime(filePath));

        filePath = Path.Combine(path, @".git\COMMIT_EDITMSG");
        if (File.Exists(filePath))
            result = Max(result, File.GetLastWriteTime(filePath));

        return result;
    }
    private static DateTime Max(DateTime value1, DateTime value2)
    {
        return value2 > value1 ? value2 : value1;
    }

    public Repository[] DiscoverRepositories(string githubContainerPath, bool nuget, IProgress<string> progress)
    {
        var repos = new List<Repository>();
        var directories = Directory.GetDirectories(githubContainerPath);
        if (directories.Any(d => Path.GetFileName(d) == ".git"))
        {
            var repo = new Repository(githubContainerPath);
            progress.Report(repo.Name);
            repos.Add(repo);
            DiscoverRepository(repo.Path, repo, nuget);
            ResolveProjectReferences(repo);
        }
        else
        {
            foreach (var dir in directories)
            {
                var repo = new Repository(dir);
                progress.Report(repo.Name);
                repos.Add(repo);
                DiscoverRepository(repo.Path, repo, nuget);
                ResolveProjectReferences(repo);
            }
        }
        return repos.ToArray();
    }
    private void DiscoverRepository(string directory, Repository repo, bool nuget)
    {
        foreach (var path in Directory.GetFiles(directory, "*.csproj"))
        {
            var project = new Project(repo, path);
            repo.Projects.Add(project);
            DiscoverProject(project, nuget);
        }

        foreach (var dir in Directory.GetDirectories(directory))
        {
            var name = Path.GetFileName(dir).ToLowerInvariant();
            if (name == ".git" || name == ".github" || name == ".vs")
                continue;
            DiscoverRepository(dir, repo, nuget);
        }
    }
    private void DiscoverProject(Project project, bool nuget)
    {
        ParseCsproj(project, nuget);

        DiscoverComponents(project.Path, project, nuget);
        foreach (var dir in Directory.GetDirectories(project.Path))
            DiscoverComponents(dir, project, nuget);
    }
    private bool ParseCsproj(Project project, bool nuget)
    {
        var xml = new XmlDocument();
        xml.Load(project.PrjPath);

        var x = xml.SelectSingleNode("/Project[@Sdk='Microsoft.NET.Sdk']");
        if (x == null)
            x = xml.SelectSingleNode("/Project[@Sdk='Microsoft.NET.Sdk.Web']");

        if (x == null)
            return false;

        var pkgId = xml.SelectSingleNode("/Project/PropertyGroup/PackageId")?.InnerText ?? project.Name;
        var pkgVersion = xml.SelectSingleNode("/Project/PropertyGroup/Version")?.InnerText;
        if (pkgVersion != null)
        {
            project.Version = pkgVersion;
            var nugetVersion = nuget ? GetNugetOrgVersion(pkgId) : PublishedVersion.Empty;
            var component = new Component(pkgId, pkgVersion, nugetVersion, project.PrjPath, project);
            project.Components.Add(component);
            //if (!_args.References)
            //    PrintComponent(component);
        }

        // ReSharper disable once PossibleNullReferenceException
        foreach (XmlElement packageElement in xml.SelectNodes("//PackageReference"))
        {
            var id = packageElement.Attributes["Include"]?.Value;
            var version = packageElement.Attributes["Version"]?.Value;
            if (!string.IsNullOrEmpty(id))
                project.Packages.Add(new Package(id, version, null, project));
        }
        // ReSharper disable once PossibleNullReferenceException
        foreach (XmlElement packageElement in xml.SelectNodes("//ProjectReference"))
        {
            var relativePath = packageElement.Attributes["Include"]?.Value;
            if (!string.IsNullOrEmpty(relativePath))
                project.ProjectReferences.Add(relativePath);
        }

        return true;
    }
    private void DiscoverComponents(string directory, Project project, bool nuget)
    {
        var nuSpecs = Directory.GetFiles(directory, "*.nuspec");
        foreach (var nuSpec in nuSpecs)
            project.Components.Add(ParseComponent(nuSpec, project, nuget));

        var path = Directory.GetFiles(directory, "packages.config").FirstOrDefault();
        if (path != null)
            project.Packages.AddRange(ParsePackages(path, project));
    }
    private Component ParseComponent(string path, Project project, bool nuget)
    {
        var xml = new XmlDocument();
        xml.Load(path);
        var nsmgr = new XmlNamespaceManager(xml.NameTable);
        nsmgr.AddNamespace("x", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");
        var p = string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI) ? "" : "x:";

        var id = xml.SelectSingleNode($"//{p}metadata/{p}id", nsmgr)?.InnerText;
        var version = xml.SelectSingleNode($"//{p}metadata/{p}version", nsmgr)?.InnerText;
        var nugetVersion = nuget ? GetNugetOrgVersion(id) : PublishedVersion.Empty;
        var component = new Component(id, version, nugetVersion, path, project);
        //if (!_args.References)
        //    PrintComponent(component);
        return component;
    }
    private static IEnumerable<Package> ParsePackages(string path, Project project)
    {
        var xml = new XmlDocument();
        xml.Load(path);
        var packages = new List<Package>();
        // ReSharper disable once PossibleNullReferenceException
        foreach (XmlElement packageElement in xml.SelectNodes("//package"))
        {
            var id = packageElement.Attributes["id"]?.Value;
            var version = packageElement.Attributes["version"]?.Value;
            var targetFramework = packageElement.Attributes["targetFramework"]?.Value;
            if (!string.IsNullOrEmpty(id))
                packages.Add(new Package(id, version, targetFramework, project));
        }

        return packages;
    }
    private void ResolveProjectReferences(Repository repo)
    {
        foreach (var project in repo.Projects)
        {
            var prjDir = Path.GetDirectoryName(project.PrjPath);
            foreach (var relativePath in project.ProjectReferences)
            {
                var targetPath = Path.GetFullPath(Path.Combine(prjDir, relativePath));
                var targetProject = repo.Projects.FirstOrDefault(p => p.PrjPath == targetPath);
                if (targetProject != null)
                    project.Dependencies.Add(targetProject);
                else
                    continue;
            }
        }
    }
    public PublishedVersion GetNugetOrgVersion(string packageId)
    {
        return _nugetTools.GetLatestVersionAsync(packageId, CancellationToken.None).GetAwaiter().GetResult();
    }
}