using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Slipe
{
    class CsprojFile
    {
        private string path;
        private XmlDocument document;
        private XmlElement root;
        private XmlElement propertyGroup;
        private XmlElement rootNameSpace;
        private XmlElement targetFramework;

        public CsprojFile(string path)
        {
            this.path = path;
        }

        public CsprojFile(string path, string rootNameSpace, string targetFramework)
        {
            this.path = path;
            document = new XmlDocument();

            root = document.CreateElement("Project");
            document.AppendChild(root);
            root.SetAttribute("Sdk", "Microsoft.NET.Sdk");

            propertyGroup = document.CreateElement("PropertyGroup");

            this.rootNameSpace = document.CreateElement("RootNamespace");
            this.rootNameSpace.InnerText = rootNameSpace;

            this.targetFramework = document.CreateElement("TargetFramework");
            this.targetFramework.InnerText = targetFramework;

            root.AppendChild(propertyGroup);
            propertyGroup.AppendChild(this.rootNameSpace);
            propertyGroup.AppendChild(this.targetFramework);
        }

        public void Save()
        {
            document.Save(this.path);
        }
    }
}
