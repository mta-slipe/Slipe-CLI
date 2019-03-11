using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Slipe.Commands.Project
{
    class GenerateMetaCommand : ProjectCommand
    {
        public override string Template => "meta-generate";

        private XmlDocument meta;
        private XmlElement root;

        public override void Run()
        {
            SlipeConfig config = ConfigHelper.Read();


            meta = new XmlDocument();
            root = meta.CreateElement("meta");

            CreateSystemElements(config);
            CreateMTAElements();
            IndexDirectory("./Dist/Server", "server");
            IndexDirectory("./Dist/Client", "client");
            CreateMainElements();



            meta.AppendChild(root);
            meta.Save("./meta.xml");

        }

        private void CreateSystemElements(SlipeConfig config)
        {
            foreach (string systemComponent in config.systemComponents)
            {
                XmlElement element = meta.CreateElement("script");
                element.SetAttribute("src", "Slipe/Lua/System/" + systemComponent);
                element.SetAttribute("type", "shared");
                root.AppendChild(element);
            }
        }

        private void CreateMTAElements()
        {
            XmlElement element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/MTA/server.lua");
            element.SetAttribute("type", "server");
            root.AppendChild(element);

            element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/MTA/client.lua");
            element.SetAttribute("type", "client");
            root.AppendChild(element);

            element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/MTA/shared.lua");
            element.SetAttribute("type", "shared");
            root.AppendChild(element);
        }


        private void IndexDirectory(string directory, string scriptType)
        {
            Console.WriteLine("Indexing {0}", directory);
            foreach(string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (! file.EndsWith("manifest.lua"))
                {
                    string relativePath = file.Replace("\\", "/");
                    if (relativePath.StartsWith("."))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    if (relativePath.StartsWith("/"))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    XmlElement element = meta.CreateElement("script");
                    element.SetAttribute("src", relativePath);
                    element.SetAttribute("type", scriptType);
                    root.AppendChild(element);
                }
            }
        }

        private void CreateMainElements()
        {
            XmlElement element = meta.CreateElement("file");
            element.SetAttribute("src", "Dist/Client/manifest.lua");
            root.AppendChild(element);

            element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/Main/main.lua");
            element.SetAttribute("type", "shared");
            root.AppendChild(element);
        }
    }
}
