using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Slipe.Commands.Project.HttpFiles
{
    class RemoveHttpDirectoryCommand : ProjectCommand
    {
        public override string Template => "remove-http";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n"  + Template + " {directory-path}");
            }

            string httpDirectory = parameters[0];

            SlipeAssetDirectory directory = new SlipeAssetDirectory
            {
                path = httpDirectory,
            };

            if (targetsModule)
            {
                targetModule.httpDirectories.RemoveAll((directory) => directory.path == httpDirectory);
            }
            else
            {
                config.httpDirectories.RemoveAll((directory) => directory.path == httpDirectory);
            }
        }
    }
}
