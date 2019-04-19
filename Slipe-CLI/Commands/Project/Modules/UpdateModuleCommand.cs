using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class UpdateModuleCommand : ProjectCommand
    {
        public override string Template => "update-module";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the module name, syntax: \n" + Template + " {filepath or url}");
            }

            string pathOrUrl = parameters[0];
            string path;

            Uri uriResult;
            bool isUrl = Uri.TryCreate(pathOrUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                path = "./DownloadedModule.zip";
                new WebClient().DownloadFile(pathOrUrl, path);
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
                    Directory.Delete(existingModule.path, true);
                    module.path = existingModule.path;
                    config.modules.Remove(existingModule);
                    break;
                }
            }

            string targetDirectory = module.path;
            Directory.Move(tempDir, targetDirectory);

            config.modules.Add(module);

            if (isUrl)
            {
                File.Delete(path);
            }
        }
    }
}
