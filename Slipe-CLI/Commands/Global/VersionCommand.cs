using SlipeUrls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Slipe.Commands.Global
{
    class VersionCommand : GlobalCommand
    {
        public override string Template => "version";
        public override CommandType CommandType => CommandType.Global;

        public override void Run()
        {
            Console.WriteLine($"\nSlipe CLI version {typeof(VersionCommand).Assembly.GetName().Version}");

        }
    }
}
