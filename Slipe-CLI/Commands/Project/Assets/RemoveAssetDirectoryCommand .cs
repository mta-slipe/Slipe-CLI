using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Slipe.Commands.Project.Assets
{
    class RemoveAssetDirectoryCommand : ProjectCommand
    {
        public override string Template => "remove-assets";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n"  + Template + " {directory-path}");
            }

            string assetDirectory = parameters[0];

            SlipeAssetDirectory directory = new SlipeAssetDirectory
            {
                path = assetDirectory,
                downloads = options.ContainsKey("no-download")
            };

            if (targetsModule)
            {
                targetModule.assetDirectories.RemoveAll((directory) => directory.path == assetDirectory);
            } else
            {
                config.assetDirectories.RemoveAll((directory) => directory.path == assetDirectory);
            }
        }
    }
}
