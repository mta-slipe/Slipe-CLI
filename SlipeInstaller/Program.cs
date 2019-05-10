using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeInstaller
{
    class Program
    {
        const string url = "https://mta-slipe.com/downloads/cli.zip"

        static void Main(string[] args)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string userPath = Path.Combine(programFiles, "Slipe");



        }
    }
}
