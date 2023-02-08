using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Slipe.Commands.Project.HttpFiles
{
    class SetDefaultHttpFile : ProjectCommand
    {
        public override string Template => "set-default-http";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n"  + Template + " {file-path}");
            }

            string httpFilePath = parameters[0];
            if (targetsModule)
            {
                throw new SlipeException("This command is not available with modules!");
            }
            else
            {
                config.defaultHttpFile = httpFilePath;
            }

            Console.WriteLine("Success. You can specify the interpreted files in .slipe file.");
        }
    }
}
