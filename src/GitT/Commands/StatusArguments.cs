using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    internal class StatusArguments
    {
        [CommandLineArgument(aliases: "F", helpText: "Fetches every repository immediately.")]
        public bool Fetch { get; set; }
    }
}
