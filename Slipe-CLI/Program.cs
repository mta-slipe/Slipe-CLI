using Slipe.Commands;
using Slipe.Commands.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Slipe
{
    class CLI
    {
        static int Main(string[] args)
        {
            try
            {
                //args = new string[]
                //{
                //    "restart-resource",
                //};
                new CLI(args);
            } catch(SlipeException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                return -1;
            }
            return 0;
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
                    if (command is ProjectCommand)
                    {
                        ((ProjectCommand)command).SaveConfig();
                    }
                    return;
                }
            }
            throw new SlipeException(string.Format("Slipe command '{0}' not found", args[0]));
        }
    }
}
