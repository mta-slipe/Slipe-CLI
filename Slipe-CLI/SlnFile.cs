using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Slipe
{
    class SlnFile
    {
        private string path;

        public SlnFile(string path)
        {
            this.path = path;
        }

        public void AddProject(string name, string path)
        {
            string content = File.ReadAllText(this.path);
            string guid = Guid.NewGuid().ToString();

            string newLine = string.Format(@"Project(""{{9A19103F-16F7-4668-BE54-9A1E7A4F7556}}"") = ""{0}"", ""{1}"", ""{{{2}}}
EndProject", name, path, guid);
            File.WriteAllText(this.path, content + "\n" + newLine);
        }

        public void RemoveProject(string name, string path)
        {
            string[] lines = File.ReadAllLines(this.path);
            string[] newLines = new string[lines.Length - 2];

            int offset = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("Project") && line.Contains(name) && line.Contains(path))
                {
                    i++;
                } else
                {
                    newLines[i - offset] = line;
                }

            }
            File.WriteAllLines(this.path, newLines);
        }
    }
}
