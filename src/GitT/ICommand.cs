using System.IO;

namespace GitT
{
    public interface ICommand
    {
        string ShortInfo { get; }
        TextReader In { get; set; }
        TextWriter Out { get; set; }

        void Execute(CommandContext context);
    }
}
