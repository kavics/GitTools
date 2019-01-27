using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GitT.Commands
{
    public class ComponentsCommand : ICommand
    {
        public string ShortInfo => "Discovers emitted Nuget packages. Optionally checks the published versions " +
                                   "in the nuget.org, and other configured locations";

        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private ComponentsArguments _args;
        public void Execute()
        {
            if (!Context.ParseArguments(out _args))
                return;
            Run(Context.GithubContainer);
        }

        private static void Run(string githubPath)
        {
            var repositories = Discover(githubPath);

            //foreach (var project in repositories.SelectMany(r => r.Projects.Where(p => p.Components.Length > 0)))
            //{

            //}
            var components = repositories.SelectMany(r => r.Projects).SelectMany(p => p.Components).ToArray();
            var packages = repositories.SelectMany(r => r.Projects).SelectMany(p => p.Packages).ToArray();

            //Console.WriteLine($"COMPONENTS");
            //Console.WriteLine();

            foreach (var c in components)
                Console.WriteLine("{0,-50} {1,-20} {2}", c.Id, c.Version, c.NugetVersion);
return;
            Console.WriteLine();
            Console.WriteLine($"INCOMPATIBLE PACKAGES");
            Console.WriteLine();

            var inCompPkgs = new List<Package>();
            foreach (var package in packages)
            {
                var component = components.FirstOrDefault(c => c.Id == package.Id);
                if (component == null)
                    continue;
                if (component.Version != package.Version)
                    inCompPkgs.Add(package);
            }

            //var x = inCompPkgs.OrderBy(p => p.Project.Name).ThenBy(p => p.Id)
            foreach (var item in inCompPkgs
                .GroupBy(p => p.Project.Name, p => p, (x, y) => new { proj = x, refs = y.ToArray() }))
            {
                Console.WriteLine(item.proj);
                foreach (var @ref in item.refs)
                    Console.WriteLine("  {0,-70} {1}", @ref.Id, @ref.Version);
            }
        }

        private static Repository[] Discover(string githubPath)
        {
            var repos = new List<Repository>();
            foreach (var dir in Directory.GetDirectories(githubPath))
            {
                var repo = new Repository(dir);
                repos.Add(repo);
                DiscoverRepository(repo.Path, repo);
            }
            return repos.ToArray();
        }

        private static void DiscoverRepository(string directory, Repository repo)
        {
            foreach (var path in Directory.GetFiles(directory, "*.csproj"))
            {
                var project = new Project(path);
                repo.Projects.Add(project);
                DiscoverProject(project);
            }

            foreach (var dir in Directory.GetDirectories(directory))
                DiscoverRepository(dir, repo);
        }

        private static void DiscoverProject(Project project)
        {
            if (ParseCsproj(project))
                return;
            DiscoverComponents(project.Path, project);
            foreach (var dir in Directory.GetDirectories(project.Path))
                DiscoverComponents(dir, project);
        }

        private static bool ParseCsproj(Project project)
        {
            var xml = new XmlDocument();
            xml.Load(project.PrjPath);

            var x = xml.SelectSingleNode("/Project[@Sdk='Microsoft.NET.Sdk']");
            if (x == null)
                return false;

            var pkgId = xml.SelectSingleNode("/Project/PropertyGroup/PackageId")?.InnerText ?? project.Name;
            var pkgVersion = xml.SelectSingleNode("/Project/PropertyGroup/Version")?.InnerText;
            project.Components.Add(new Component(pkgId, pkgVersion, project.PrjPath, project));

            // ReSharper disable once PossibleNullReferenceException
            foreach (XmlElement packageElement in xml.SelectNodes("//PackageReference"))
            {
                var id = packageElement.Attributes["Include"]?.Value;
                var version = packageElement.Attributes["Version"]?.Value;
                if (!string.IsNullOrEmpty(id))
                    project.Packages.Add(new Package(id, version, null, project));
            }


            return true;
        }

        private static void DiscoverComponents(string directory, Project project)
        {
            var path = Directory.GetFiles(directory, "*.nuspec").FirstOrDefault();
            if (path != null)
                project.Components.Add(ParseComponent(path, project));

            path = Directory.GetFiles(directory, "packages.config").FirstOrDefault();
            if (path != null)
                project.Packages.AddRange(ParsePackages(path, project));
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static Component ParseComponent(string path, Project project)
        {
            var xml = new XmlDocument();
            xml.Load(path);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");
            var p = string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI) ? "" : "x:";

            var id = xml.SelectSingleNode($"//{p}metadata/{p}id", nsmgr)?.InnerText;
            var version = xml.SelectSingleNode($"//{p}metadata/{p}version", nsmgr)?.InnerText;
            return new Component(id, version, path, project);
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

    }
}
