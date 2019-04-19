using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class ImportModuleCommand : ProjectCommand
    {
        public override string Template => "import-module";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the module name, syntax: \n" + Template + " {filepath or url} [-directory {directory}]");
            }

            string pathOrUrl = parameters[0];
            string path;

            Uri uriResult;
            bool isUrl = Uri.TryCreate(pathOrUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                path = "/DownloadedModule.zip";
                new WebClient().DownloadFile("http://mta-slipe.com/slipe-core.zip", path);
            } else
            {
                path = pathOrUrl;
            }

            if (!File.Exists(path))
            {
                throw new SlipeException("Unable to find file at " + path);
            }
            string tempDir = path.Replace(".zip", "");

            ZipFile.ExtractToDirectory(path, tempDir);

            SlipeModule module = ConfigHelper.ReadModule(tempDir + "/.slipe");

            foreach(SlipeModule existingModule in config.modules)
            {
                if (existingModule.name == module.name)
                {
                    if (isUrl)
                    {
                        Directory.Delete(path, true);
                    }
                    Directory.Delete(tempDir, true);
                    throw new SlipeException("A module with the name '" + module.name + "' already exists, use `slipe update-module` if you wish to update this module");
                }
            }

            string targetDirectory = "./Modules/" + module.name;
            if (options.ContainsKey("directory"))
            {
                targetDirectory = options["directory"];
            }

            Directory.Move(tempDir, targetDirectory);

            config.modules.Add(module);

            if (isUrl)
            {
                Directory.Delete(path, true);
            }
        }
    }
}
