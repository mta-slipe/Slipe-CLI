using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe.Commands.Project.HttpFiles
{
    class AddHttpDirectoryCommand : ProjectCommand
    {
        public override string Template => "add-http";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n"  + Template + " {directory-path}");
            }

            string httpDirectory = parameters[0];

            SlipeHttpDirectory directory = new SlipeHttpDirectory
            {
                path = httpDirectory,
            };

            if (targetsModule)
            {
                targetModule.httpDirectories.Add(directory);
            }
            else
            {
                config.httpDirectories.Add(directory);
            }

            Console.WriteLine("Success. You can specify the interpreted files in .slipe file.");
        }
    }
}
