using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    internal class ComponentsArguments
    {
        [CommandLineArgument(aliases: "n,N", helpText: "Fetches every component version from nuget.org.")]
        public bool Nuget { get; set; }
        [CommandLineArgument(aliases: "l,L", helpText: "Fetches every component version from local nugets.")]
        public bool Local { get; set; }
    }
}
