using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using GitT.Models;
using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    // ReSharper disable once UnusedMember.Global
    public class ComponentsCommand : ICommand
    {
        public string ShortInfo => "Discovers emitted/referenced Nuget packages. " +
                                   "Optionally checks the published versions in the nuget.org, " +
                                   "and other configured locations";

        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private ComponentsArguments _args;
        private INugetTools _nugetTools;


        public ComponentsCommand(INugetTools nugetTools)
        {
            _nugetTools = nugetTools;
        }

        public void Execute()
        {
            try
            {
                if (!Context.ParseArguments(out _args))
                    return;
            }
            catch (ParsingException e)
            {
                Console.WriteLine(e.FormattedMessage);
                return;
            }

            if (_args.References)
                _args.Nuget = false;

            Run();
        }

        private void Run()
        {
            if (_args.References)
            {
                Console.WriteLine("REFERENCES");
            }
            else
            {
                Console.WriteLine("COMPONENTS");
                if (_args.Nuget)
                {
                    Console.WriteLine("{0,-50} {1,-15} {2}", "Component.Id", "Version", "nuget.org");
                    Console.WriteLine("================================================== =============== ===============");
                }
                else
                {
                    Console.WriteLine("{0,-50} {1,-15}", "Component.Id", "Version");
                    Console.WriteLine("================================================== ===============");
                }
            }
            var repositories = Discover();

            if (_args.Differences)
            {
                var components = repositories.SelectMany(r => r.Projects).SelectMany(p => p.Components).ToArray();
                var packages = _args.References
                    ? repositories.SelectMany(r => r.Projects).SelectMany(p => p.Packages).ToArray()
                    : new Package[0];

                Console.WriteLine();
                Console.WriteLine("INCOMPATIBLE PACKAGES");
                Console.WriteLine();

                var incompatiblePackages = new List<Package>();
                if (!string.IsNullOrEmpty(_args.Prefix))
                    packages = packages
                        .Where(p => p.Id.StartsWith(_args.Prefix, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                foreach (var package in packages)
                {
                    var component = components.FirstOrDefault(c => c.Id == package.Id);
                    if (component == null)
                        continue;
                    if (component.Version != package.Version)
                        incompatiblePackages.Add(package);
                }

                //var x = incompatiblePackages.OrderBy(p => p.Project.Name).ThenBy(p => p.Id)
                foreach (var item in incompatiblePackages
                    .GroupBy(p => p.Project.Name, p => p, (x, y) => new { proj = x, refs = y.ToArray() }))
                {
                    Console.WriteLine(item.proj);
                    foreach (var @ref in item.refs)
                        Console.WriteLine("  {0,-50} {1}", @ref.Id, @ref.Version);
                }
            }
        }

        private Repository[] Discover()
        {
            var repos = new List<Repository>();
            foreach (var dir in Directory.GetDirectories(Context.GithubContainer))
            {
                var repo = new Repository(dir);
                repos.Add(repo);
                DiscoverRepository(repo.Path, repo);
                ResolveProjectReferences(repo);
                if (_args.References && !_args.Differences)
                    foreach (var project in repo.Projects)
                        PrintProjectReferences(project);
            }
            return repos.ToArray();
        }
        private void PrintProjectReferences(Project project)
        {
            var refs = GetFilteredReferences(project);
            if (!refs.Any() && !project.Dependencies.Any())
                return;

            var root = Context.GithubContainer + "\\";

            Console.WriteLine("{0} - {1} - {2}", project.Name, project.Version, project.Path.Replace(root, string.Empty));
            foreach (var dependency in project.Dependencies)
                Console.WriteLine("    {0,-60} {1,-15}", dependency.Name, dependency.Version);
            foreach (var package in refs)
                Console.WriteLine("    {0,-60} {1,-15}", package.Id, package.Version);
        }
        private List<Package> GetFilteredReferences(Project project)
        {
            var packages = project.Packages;
            var prefix = _args.Prefix;

            if (!string.IsNullOrEmpty(prefix))
                packages = packages
                    .Where(x => x.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return packages;
        }

        private void DiscoverRepository(string directory, Repository repo)
        {
            foreach (var path in Directory.GetFiles(directory, "*.csproj"))
            {
                var project = new Project(repo, path);
                repo.Projects.Add(project);
                DiscoverProject(project);
            }

            foreach (var dir in Directory.GetDirectories(directory))
                DiscoverRepository(dir, repo);
        }

        private void DiscoverProject(Project project)
        {
            ParseCsproj(project);

            DiscoverComponents(project.Path, project);
            foreach (var dir in Directory.GetDirectories(project.Path))
                DiscoverComponents(dir, project);
        }

        private bool ParseCsproj(Project project)
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
                var nugetVersion = _args.Nuget ? GetNugetOrgVersion(pkgId) : string.Empty;
                var component = new Component(pkgId, pkgVersion, nugetVersion, project.PrjPath, project);
                project.Components.Add(component);
                if (!_args.References)
                    PrintComponent(component);
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

        private void DiscoverComponents(string directory, Project project)
        {
            var nuSpecs = Directory.GetFiles(directory, "*.nuspec");
            foreach (var nuSpec in nuSpecs)
                project.Components.Add(ParseComponent(nuSpec, project));

            var path = Directory.GetFiles(directory, "packages.config").FirstOrDefault();
            if (path != null)
                project.Packages.AddRange(ParsePackages(path, project));
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private Component ParseComponent(string path, Project project)
        {
            var xml = new XmlDocument();
            xml.Load(path);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");
            var p = string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI) ? "" : "x:";

            var id = xml.SelectSingleNode($"//{p}metadata/{p}id", nsmgr)?.InnerText;
            var version = xml.SelectSingleNode($"//{p}metadata/{p}version", nsmgr)?.InnerText;
            var nugetVersion = _args.Nuget ? GetNugetOrgVersion(id) : string.Empty;
            var component = new Component(id, version, nugetVersion, path, project);
            if (!_args.References)
                PrintComponent(component);
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

        private static void PrintComponent(Component component)
        {
            Console.WriteLine("{0,-50} {1,-15} {2}", component.Id, component.Version, component.NugetVersion);
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

        public string GetNugetOrgVersion(string packageId)
        {
            return _nugetTools.GetLatestVersionAsync(packageId, CancellationToken.None).GetAwaiter().GetResult() ?? string.Empty;
        }

    }
}
