using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitT.Properties;
using SenseNet.Tools.CommandLineArguments;

namespace GitT.Commands
{
    public class ConfigureCommand : ICommand
    {
        public string ShortInfo => "Queries or customizes Git Tools specialities.";
        public TextReader In { get; set; }
        public TextWriter Out { get; set; }
        public CommandContext Context { get; set; }

        private ConfigureArguments _args;
        public void Execute()
        {
            try
            {
                if (!Context.ParseArguments<ConfigureArguments>(out _args))
                    return;
            }
            catch (ParsingException e)
            {
                Console.WriteLine(e.FormattedMessage);
                return;
            }

            var settings = Properties.Settings.Default;
            var changed = false;
            if (!string.IsNullOrEmpty(_args.GitExe))
            {
                settings.GitExe = _args.GitExe;
                changed = true;
            }
            if (!string.IsNullOrEmpty(_args.InternalNuget))
            {
                settings.InternalNuget = _args.InternalNuget;
                changed = true;
            }
            if (!string.IsNullOrEmpty(_args.PrivateNuget))
            {
                settings.PrivateNuget = _args.PrivateNuget;
                changed = true;
            }

            if (changed)
                settings.Save();
            else
                Query(settings);
        }

        private void Query(Settings settings)
        {
            var keys = new[] { "GitExe", "InternalNuget", "PrivateNuget" };
            foreach (var key in keys)
            {
                var value = settings[key].ToString();
                Console.WriteLine("{0,-20}{1}", key, value.Length > 0 ? value : "(not configured yet)");
            }
        }
    }
}
