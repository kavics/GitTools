using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands;

internal class RepositoriesArguments
{
    [NoNameOption(order: 0, required: true, nameInHelp: "Name", helpText: "Name of the organization or user.")]
    public string OrganizationName { get; set; } = "<<<unknown>>>";

    [CommandLineArgument(aliases: "B", helpText: "Show all branches of every repository.")]
    public bool Branches { get; set; }

    [CommandLineArgument(aliases: "I", helpText: "Show all issue titles of every repository.")]
    public bool Issues { get; set; }
}