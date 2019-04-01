using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class RemoveModuleCommand : ProjectCommand
    {
        public override string Template => "remove-module";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n" + Template + " {name}");
            }
            string name = parameters[0];

            config.modules.RemoveAll((module) => module.name == name);
        }
    }
}
