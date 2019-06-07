using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Global.Analytics
{
    class OptOutCommand: GlobalCommand
    {
        public override string Template => "opt-out";
        public override CommandType CommandType => CommandType.Global;

        public override void Run()
        {
            string path = CLI.GetAnalyticsPreferencesFilePath();
            File.WriteAllText(path, "no");
        }
    }
}
