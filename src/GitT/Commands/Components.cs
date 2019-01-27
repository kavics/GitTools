using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitT.Commands
{
    public class Components : ICommand
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

        }
    }
}
