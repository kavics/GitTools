using Kavics.GittLib;
using Kavics.GittLib.Controllers;
using Kavics.GittLib.Models;
using NuGet.Packaging.Signing;
using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    // GitT components [-Differences:Boolean] [-Nuget:Boolean] [-Prefix:String] [-References:Boolean] [?]
    public class ComponentsCommand : ICommand
    {
        public string ShortInfo => "Discovers emitted/referenced Nuget packages. " +
                                   "Optionally checks the published versions in the nuget.org, " +
                                   "and other configured locations";

        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private ComponentsArguments _args;
        private readonly INugetTools _nugetTools;
        private readonly ILocalRepositoryController _localRepositoryController;


        public ComponentsCommand(INugetTools nugetTools, ILocalRepositoryController controller)
        {
            _nugetTools = nugetTools;
            _localRepositoryController = controller;
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

            if (_args.Differences || _args.Graph || _args.SimulateRelease || _args.References)
                _args.Nuget = false;

            Run();
        }

        private void Run()
        {
            if (_args.SimulateRelease)
            {
                Console.WriteLine("COMPUTE RELEASE WORKFLOW");
            }
            else if (_args.References)
            {
                Console.WriteLine("REFERENCES");
            }
            else if (_args.Graph)
            {
                Console.WriteLine("DEPENDENCY GRAPH");
            }
            else
            {
                Console.WriteLine("COMPONENTS");
                if (_args.Nuget)
                {
                    Console.WriteLine("{0,-24} {1,-50} {2,-13} {3,-13} {4}", "Repository", "Component.Id", "Version", "nuget.org", "Published");
                    Console.WriteLine("======================== ================================================== ============= ============= ===========");
                }
                else
                {
                    Console.WriteLine("{0,-24} {1,-50} {2,-13}", "Repository", "Component.Id", "Version");
                    Console.WriteLine("======================== ================================================== ===============");
                }
            }

            var progress = new Progress<string>(name => { Console.Write($"Discover {name}                            \r"); });
            var repositories = _localRepositoryController
                .DiscoverRepositories(Context.GithubContainer, _args.Nuget, progress);
            Console.Write("                                                     \r");

            if (_args.Differences) // "components -diff"
            {
                var components = repositories.SelectMany(r => r.Projects).SelectMany(p => p.Components).ToArray();
                //var packages = _args.References
                //    ? repositories.SelectMany(r => r.Projects).SelectMany(p => p.Packages).ToArray()
                //    : new Package[0];
                var packages =
                    repositories.SelectMany(r => r.Projects).SelectMany(p => p.Packages).ToArray();

                Console.WriteLine("DIFFERENT PACKAGES");
                Console.WriteLine();

                var incompatiblePackages = new List<(Package package, string version)>();
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
                        incompatiblePackages.Add((package, component.Version));
                }

                //var x = incompatiblePackages.OrderBy(p => p.Project.Name).ThenBy(p => p.Id)
                Console.WriteLine("Project");
                Console.WriteLine("{0,-64} {1,-16} {2}", "    Component.Id", "Behind", "Latest");
                Console.WriteLine("===============================================================  ===============  ===============");
                foreach (var item in incompatiblePackages
                             .GroupBy(p => p.package.Project.Name, p => p, (x, y) => new { proj = x, refs = y.ToArray() }))
                {
                    Console.WriteLine(item.proj);
                    foreach (var @ref in item.refs)
                        Console.WriteLine("    {0,-60} {1,-16} {2}", @ref.package.Id, @ref.package.Version, @ref.version);
                }
            }
            else if (_args.SimulateRelease) // components -RSim
            {
                Console.WriteLine();
                Console.WriteLine("======================= REPOSITORIES TO RELEASE");

                var reposToRelease = new Dictionary<Repository, Component[]>();
                var componentsToRelease = new Dictionary<Component, Repository>();
                foreach (var repo in repositories)
                {
                    var cmpToRelease = repo.Projects
                        .SelectMany(p => p.Components)
                        .Where(c => c.Version.Split('.').Length == 4).ToArray();
                    if (cmpToRelease.Length > 0)
                        reposToRelease.Add(repo, cmpToRelease);
                    foreach (var component in cmpToRelease)
                        componentsToRelease.Add(component, repo);
                }
                var componentNamesToRelease = componentsToRelease.Keys
                    .Select(x => x.Name)
                    .ToList();

                foreach (var item in reposToRelease)
                {
                    Console.WriteLine(item.Key.Name);
                    foreach (var component in item.Value)
                    {
                        Console.WriteLine("    {0,-64} {1,-15}", component.Name, component.Version);
                        var x = component.Project.Packages
                            .Where(pkg => componentNamesToRelease.Contains(pkg.Id));
                        foreach (var dep in x)
                            Console.WriteLine("        {0,-60}", dep.Id);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("======================= RELEASE WORKFLOW");
                var releasedRepositories = new Dictionary<Repository, Component[]>();
                var releasedComponentNames = new List<string>();
                var repos = reposToRelease.ToList();
                while (repos.Count > 0)
                {
                    for (int i = 0; i < repos.Count; i++)
                    {
                        var repo = repos[i].Key;
                        var components = repos[i].Value;
                        var dependentComponents = components
                            .SelectMany(c => c.Project.Packages)
                            .Where(pkg => componentNamesToRelease.Contains(pkg.Id))
                            .ToArray();
                        if (!dependentComponents.Any())
                        {
                            releasedRepositories.Add(repo, components);
                            repos.Remove(repos[i]);
                            foreach (var component in components)
                            {
                                componentNamesToRelease.Remove(component.Name);
                                releasedComponentNames.Add(component.Name);
                            }
                            break;
                        }
                    }
                }
                Console.WriteLine();
                Console.WriteLine("----------------------- PART-1: RELEASE PACKAGES");
                foreach (var item in releasedRepositories)
                {
                    Console.WriteLine(item.Key.Name);
                    foreach (var component in item.Value)
                        Console.WriteLine("    {0,-64} {1,-15} -> {2,-15}", component.Name, component.Version, GetVersionToRelease(component.Version));
                }

                Console.WriteLine();
                Console.WriteLine("----------------------- PART-2: UPDATE NOT PUBLISHED PROJECTS");
                foreach (var repo in releasedRepositories.Keys)
                {
                    var projectsToUpgrade = repo.Projects
                        .Where(prj => prj.Components.Count == 0) // only not publishable projects
                        .Where(prj => prj.Packages
                            .Select(p => p.Id)
                            .Intersect(releasedComponentNames)
                            .Any())
                        .ToArray();
                    if (projectsToUpgrade.Any())
                    {
                        Console.WriteLine(repo.Name);
                        foreach (var project in projectsToUpgrade)
                        {
                            Console.WriteLine("    {0,-64}", project.Name);
                            var x = project.Packages.Select(p => p.Id)
                                .Intersect(releasedComponentNames);
                            foreach (var newPackage in x)
                                Console.WriteLine("        {0,-64}", newPackage);
                        }
                    }
                }
                Console.WriteLine();
            }
            else if (_args.References) // components -refs
            {
                foreach (var repo in repositories)
                {
                    foreach (var project in repo.Projects)
                        PrintProjectReferences(project, repositories);
                }
            }
            else if (_args.Graph) // components -graph
            {
                var data = new Dictionary<string, List<Project>>();
                foreach (var repo in repositories)
                {
                    foreach (var project in repo.Projects)
                    {
                        var allRefs = GetFilteredReferences(project).Select(x => (x.Id))
                            .Union(project.Dependencies.Select(x => (x.Name))).Distinct().ToArray();
                        foreach (var @ref in allRefs)
                        {
                            if (!data.TryGetValue(@ref, out var projects))
                            {
                                projects = new List<Project>();
                                data.Add(@ref, projects);
                            }
                            projects.Add(project);
                        }
                    }
                }
                PrintDependencyGraph(data, repositories);
            }
            else // components | components -nuget
            {
                foreach (var repo in repositories)
                    foreach (var project in repo.Projects)
                        foreach (var component in project.Components)
                            PrintComponent(component);
            }
        }

        private string GetVersionToRelease(string version)
        {
            var segments = version.Split('.');
            if (segments.Length < 4)
                return version;
            var newSegments = segments.Take(2).ToList();
            if (int.TryParse(segments[2], out var segment2))
                newSegments.Add((segment2 + 1).ToString());
            else
                newSegments.Add("???");
            return string.Join(".", newSegments);
        }

        private void PrintDependencyGraph(Dictionary<string, List<Project>> reverseReferences, Repository[] allRepositories)
        {
            Console.WriteLine("What additional components should be updated if the current component is updated.");
            foreach (var item in reverseReferences)
            {
                var componentName = item.Key;
                var component = allRepositories
                    .SelectMany(repo => repo.Projects)
                    .SelectMany(prj => prj.Components)
                    .FirstOrDefault(c => c.Id == item.Key);
                    Console.WriteLine("{0} - {1} - {2}", componentName, component?.Version, component?.Project.Repository.Name);
                foreach (var dependency in item.Value)
                    Console.WriteLine("    {0,-60} {1,-15} {2}", dependency.Name, dependency.Version, dependency.Repository.Name);
            }
        }

        //private Repository[] Discover()
        //{
        //    var repos = new List<Repository>();
        //    var directories = Directory.GetDirectories(Context.GithubContainer);
        //    if (directories.Any(d => Path.GetFileName(d) == ".git"))
        //    {
        //        var repo = new Repository(Context.GithubContainer);
        //        repos.Add(repo);
        //        DiscoverRepository(repo.Path, repo);
        //        ResolveProjectReferences(repo);
        //    }
        //    else
        //    {
        //        foreach (var dir in directories)
        //        {
        //            var repo = new Repository(dir);
        //            Console.Write($"Discover {repo.Name}                            \r");
        //            repos.Add(repo);
        //            DiscoverRepository(repo.Path, repo);
        //            ResolveProjectReferences(repo);
        //        }
        //        Console.Write("                                                     \r");
        //    }
        //    return repos.ToArray();
        //}
        private void PrintProjectReferences(Project project, Repository[] allRepositories)
        {
            var refs = GetFilteredReferences(project);
            if (!refs.Any() && !project.Dependencies.Any())
                return;

            var root = Context.GithubContainer + "\\";

            //Console.WriteLine("{0} - {1} - {2}", project.Name, project.Version, project.Path.Replace(root, string.Empty));
            Console.WriteLine("{0} - {1} - {2}", project.Name, project.Version, project.Repository.Name);
            foreach (var dependency in project.Dependencies)
                Console.WriteLine("    {0,-60} {1,-15}", dependency.Name, dependency.Version);
            foreach (var package in refs)
                Console.WriteLine("    {0,-60} {1,-15} {2}", package.Id, package.Version, GetRepositoryName(package, project.Repository.Name, allRepositories));
        }

        private string GetRepositoryName(Package package, string currentRepositoryName, Repository[] allRepositories)
        {
            var packageId = package.Id;
            var repo = allRepositories.FirstOrDefault(repo =>
                repo.Projects.Any(project =>
                    project.Components.Any(cmp => cmp.Id == packageId)));
            var name = repo?.Name ?? string.Empty;
            return currentRepositoryName == name ? string.Empty : name;
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

        //private void DiscoverRepository(string directory, Repository repo)
        //{
        //    foreach (var path in Directory.GetFiles(directory, "*.csproj"))
        //    {
        //        var project = new Project(repo, path);
        //        repo.Projects.Add(project);
        //        DiscoverProject(project);
        //    }

        //    foreach (var dir in Directory.GetDirectories(directory))
        //        DiscoverRepository(dir, repo);
        //}

        //private void DiscoverProject(Project project)
        //{
        //    ParseCsproj(project);

        //    DiscoverComponents(project.Path, project);
        //    foreach (var dir in Directory.GetDirectories(project.Path))
        //        DiscoverComponents(dir, project);
        //}

        //private bool ParseCsproj(Project project)
        //{
        //    var xml = new XmlDocument();
        //    xml.Load(project.PrjPath);

        //    var x = xml.SelectSingleNode("/Project[@Sdk='Microsoft.NET.Sdk']");
        //    if (x == null)
        //        x = xml.SelectSingleNode("/Project[@Sdk='Microsoft.NET.Sdk.Web']");

        //    if (x == null)
        //        return false;

        //    var pkgId = xml.SelectSingleNode("/Project/PropertyGroup/PackageId")?.InnerText ?? project.Name;
        //    var pkgVersion = xml.SelectSingleNode("/Project/PropertyGroup/Version")?.InnerText;
        //    if (pkgVersion != null)
        //    {
        //        project.Version = pkgVersion;
        //        var nugetVersion = _args.Nuget ? GetNugetOrgVersion(pkgId) : PublishedVersion.Empty;
        //        var component = new Component(pkgId, pkgVersion, nugetVersion, project.PrjPath, project);
        //        project.Components.Add(component);
        //        //if (!_args.References)
        //        //    PrintComponent(component);
        //    }

        //    // ReSharper disable once PossibleNullReferenceException
        //    foreach (XmlElement packageElement in xml.SelectNodes("//PackageReference"))
        //    {
        //        var id = packageElement.Attributes["Include"]?.Value;
        //        var version = packageElement.Attributes["Version"]?.Value;
        //        if (!string.IsNullOrEmpty(id))
        //            project.Packages.Add(new Package(id, version, null, project));
        //    }
        //    // ReSharper disable once PossibleNullReferenceException
        //    foreach (XmlElement packageElement in xml.SelectNodes("//ProjectReference"))
        //    {
        //        var relativePath = packageElement.Attributes["Include"]?.Value;
        //        if (!string.IsNullOrEmpty(relativePath))
        //            project.ProjectReferences.Add(relativePath);
        //    }

        //    return true;
        //}

        //private void DiscoverComponents(string directory, Project project)
        //{
        //    var nuSpecs = Directory.GetFiles(directory, "*.nuspec");
        //    foreach (var nuSpec in nuSpecs)
        //        project.Components.Add(ParseComponent(nuSpec, project));

        //    var path = Directory.GetFiles(directory, "packages.config").FirstOrDefault();
        //    if (path != null)
        //        project.Packages.AddRange(ParsePackages(path, project));
        //}

        //[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        //private Component ParseComponent(string path, Project project)
        //{
        //    var xml = new XmlDocument();
        //    xml.Load(path);
        //    var nsmgr = new XmlNamespaceManager(xml.NameTable);
        //    nsmgr.AddNamespace("x", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");
        //    var p = string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI) ? "" : "x:";

        //    var id = xml.SelectSingleNode($"//{p}metadata/{p}id", nsmgr)?.InnerText;
        //    var version = xml.SelectSingleNode($"//{p}metadata/{p}version", nsmgr)?.InnerText;
        //    var nugetVersion = _args.Nuget ? GetNugetOrgVersion(id) : PublishedVersion.Empty;
        //    var component = new Component(id, version, nugetVersion, path, project);
        //    //if (!_args.References)
        //    //    PrintComponent(component);
        //    return component;
        //}

        //private static IEnumerable<Package> ParsePackages(string path, Project project)
        //{
        //    var xml = new XmlDocument();
        //    xml.Load(path);
        //    var packages = new List<Package>();
        //    // ReSharper disable once PossibleNullReferenceException
        //    foreach (XmlElement packageElement in xml.SelectNodes("//package"))
        //    {
        //        var id = packageElement.Attributes["id"]?.Value;
        //        var version = packageElement.Attributes["version"]?.Value;
        //        var targetFramework = packageElement.Attributes["targetFramework"]?.Value;
        //        if (!string.IsNullOrEmpty(id))
        //            packages.Add(new Package(id, version, targetFramework, project));
        //    }

        //    return packages;
        //}

        private static void PrintComponent(Component component)
        {
            var published = component.NugetVersion.PublishedDate;
            var publishedString = published == DateTimeOffset.MinValue
                ? string.Empty
                : published.ToString("yyyy-MM-dd");
            Console.WriteLine("{0,-24} {1,-50} {2,-13} {3,-13} {4}",component.Project.Repository.Name , component.Id, component.Version, component.NugetVersion.Version, publishedString);
        }

        //private void ResolveProjectReferences(Repository repo)
        //{
        //    foreach (var project in repo.Projects)
        //    {
        //        var prjDir = Path.GetDirectoryName(project.PrjPath);
        //        foreach (var relativePath in project.ProjectReferences)
        //        {
        //            var targetPath = Path.GetFullPath(Path.Combine(prjDir, relativePath));
        //            var targetProject = repo.Projects.FirstOrDefault(p => p.PrjPath == targetPath);
        //            if (targetProject != null)
        //                project.Dependencies.Add(targetProject);
        //            else
        //                continue;
        //        }
        //    }
        //}

        //public PublishedVersion GetNugetOrgVersion(string packageId)
        //{
        //    return _nugetTools.GetLatestVersionAsync(packageId, CancellationToken.None).GetAwaiter().GetResult();
        //}

    }
}
