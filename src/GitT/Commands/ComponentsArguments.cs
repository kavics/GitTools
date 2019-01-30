using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    internal class ComponentsArguments
    {
        [CommandLineArgument(aliases: "N", helpText: "Fetches every component version from nuget.org.")]
        public bool Nuget { get; set; }

        [CommandLineArgument(aliases: "R,Ref,Refs", helpText: "Fetches every component version from nuget.org.")]
        public bool References { get; set; }

        [CommandLineArgument(aliases: "D,Diff", helpText: "Lists only the package that is different from the emitted package. Can be used only with references.")]
        public bool Differences { get; set; }

        [CommandLineArgument(aliases: "P", helpText: "Lists only the package whose identifier begins with this prefix.")]
        public string Prefix { get; set; }

    }
}
