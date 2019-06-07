using Newtonsoft.Json;
using Slipe.Commands;
using Slipe.Commands.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Slipe
{
    class CLI
    {
        public Task analyticsTask;
        private string[] args;

        static int Main(string[] args)
        {
            CLI cli = null;
            try
            {
                //args = new string[]
                //{
                //    "hook",

                //    "-domain", "127.0.0.1",
                //    "-port", "50456",

                //    "-sourceDirectory", @"C:\Program Files (x86)\MTA San Andreas 1.5\server\mods\deathmatch\resources\[slipe]\SlipeRace",
                //    "-outputDirectory", @"C:\Program Files (x86)\MTA San Andreas 1.5\server\mods\deathmatch\resources\[slipe]\[built]\SlipeRaceBuilt",

                //    "-luac",
                //    "-repo", "testRepo",
                //    "-branch", "refs/heads/master",
                //};
                cli = new CLI(args);
                cli.Run();
            } catch(SlipeException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                cli?.analyticsTask?.Wait();
                return -1;
            }
            cli?.analyticsTask?.Wait();
            return 0;
        }

        public CLI(string[] args)
        {
            this.args = args;
            analyticsTask = HandleAnalytics(args);
        }

        public void Run()
        {
            if (args.Length < 1)
            {
                throw new SlipeException("Please specify a command, Syntax: \nslipe {command}");
            }

            foreach (Type type in
                Assembly.GetAssembly(typeof(Command)).GetTypes()
                .Where(
                    myType => myType.IsClass &&
                    !myType.IsAbstract &&
                    myType.IsSubclassOf(typeof(Command)
                )))
            {
                Command command = (Command)Activator.CreateInstance(type);
                if (command.Matches(args))
                {
                    bool isProject = File.Exists("./.slipe");
                    switch (command.CommandType)
                    {
                        case CommandType.NonProject:
                            if (isProject)
                            {
                                throw new SlipeException(string.Format("'{0}' can not be executed in a slipe project directory", args[0]));
                            }
                            break;
                        case CommandType.Project:
                            // force slipe file to update to latest structure
                            ConfigHelper.Write(ConfigHelper.Read());
                            if (!isProject)
                            {
                                throw new SlipeException(string.Format("'{0}' can only be executed in a slipe project directory", args[0]));
                            }
                            break;
                    }

                    List<string> arguments = new List<string>(args);
                    arguments.RemoveAt(0);
                    command.ParseArguments(arguments.ToArray());
                    command.Run();
                    if (command is ProjectCommand)
                    {
                        ((ProjectCommand)command).SaveConfig();
                    }
                    return;
                }
            }
            throw new SlipeException(string.Format("Slipe command '{0}' not found", args[0]));
        }

        private async Task HandleAnalytics(string[] args)
        {
            try
            {
                string path = GetAnalyticsPreferencesFilePath();
                if (!File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(@"Hey there!
We appreciate you using Slipe. Would you like to send us some (anonymous) usage data?
All we store is how and when the Slipe CLI is used, no personally identifiable information will be stored.
If so, run `slipe opt-in` and we'll be forever grateful.
You will be able to stop sending usage data at any time using `slipe opt-out`");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    File.WriteAllText(path, "no");
                }
                else
                {
                    if (File.ReadAllText(path) == "yes")
                    {

                        // hash the directory name as project identifier
                        string directory = Directory.GetCurrentDirectory();
                        var sha = SHA512.Create();
                        byte[] projectHash = sha.ComputeHash(Encoding.UTF8.GetBytes(directory));

                        string command = "slipe " + string.Join(" ", args);
                        string payload = JsonConvert.SerializeObject(new
                        {
                            project = Convert.ToBase64String(projectHash),
                            command = command
                        });

                        StringContent content = new StringContent(payload);

                        HttpClient client = new HttpClient()
                        {
                            Timeout = TimeSpan.FromMilliseconds(1000)
                        };
                        var response = await client.PostAsync("https://analytics.mta-slipe.com", content);
                    }
                }
            } catch (Exception)
            {
                // quietly fail
            }
        }

        public static string GetAnalyticsPreferencesFilePath()
        {
            string directory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "Slipe");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string file = Path.Join(directory, "analytics");
            return file;
        }
    }
}
