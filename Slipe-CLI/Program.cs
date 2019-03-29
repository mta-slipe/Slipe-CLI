using Slipe.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Slipe
{
    class CLI
    {
        static void Main(string[] args)
        {
            try
            {
                //args = new string[]
                //{
                //    "create-project",
                //    "TestProject",
                //    "-y",
                //    "-module",
                //    "SlipeCore",
                //    "-server"
                //};
                new CLI(args);
            } catch(SlipeException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public CLI(string[] args)
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
                    if (command.IsProjectCommand != File.Exists("./.slipe"))
                    {
                        if (command.IsProjectCommand)
                        {
                            throw new SlipeException(string.Format("'{0}' can only be executed in a slipe project directory", args[0]));
                        } else
                        {
                            throw new SlipeException(string.Format("'{0}' can not be executed in a slipe project directory", args[0]));
                        }
                    }

                    List<string> arguments = new List<string>(args);
                    arguments.RemoveAt(0);
                    command.ParseArguments(arguments.ToArray());
                    command.Run();
                    return;
                }
            }
            throw new SlipeException(string.Format("Slipe command '{0}' not found", args[0]));
        }
    }
}
