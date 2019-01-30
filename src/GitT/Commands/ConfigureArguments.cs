using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    internal class ConfigureArguments
    {
        [CommandLineArgument(aliases: "G,Git", helpText: "Customizes the full path of the 'git.exe' on the local filesystem.")]
        public string GitExe { get; set; }

        [CommandLineArgument(aliases: "I,Internal", helpText: "Customizes the full path of the internal corporate wide nuget container.")]
        public string InternalNuget { get; set; }

        [CommandLineArgument(aliases: "P,Private", helpText: "Customizes the full path of the private nuget container on the local filesystem.")]
        public string PrivateNuget { get; set; }
    }
}
