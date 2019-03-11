using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    abstract class ProjectCommand : Command
    {
        public override bool IsProjectCommand => true;
        

        public override void ParseArguments(string[] args)
        {
            base.ParseArguments(args);
        }
    }
}
