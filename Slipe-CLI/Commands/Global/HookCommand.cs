using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Slipe.Commands.Global
{
    class HookCommand : Command
    {
        public override string Template => "hook";

        public override CommandType CommandType => CommandType.Global;


        private string protocol = "http";
        private string domain = "127.0.0.1";
        private int port = 80;

        private string repo;
        private string branchRef = "refs/heads/master";

        private string sourceDirectory = "./";
        private string outputDirectory;
        private bool luac = false;

        private bool isGithub;
        private bool pulls;

        private bool queue;
        private bool inQueue;

        private string postBuildCommand;
        private string failBuildCommand;


        private HttpListener listener;
        private int state;


        public override void Run()
        {
            state = 0;

            protocol = options.ContainsKey("protocol") ? options["protocol"] : protocol;
            domain = options.ContainsKey("domain") ? options["domain"] : domain;
            port = options.ContainsKey("port") ? int.Parse(options["port"]) : port;

            repo = options.ContainsKey("repo") ? options["repo"] : repo;
            branchRef = options.ContainsKey("branch") ? options["branch"] : branchRef;

            sourceDirectory = options.ContainsKey("sourceDirectory") ? options["sourceDirectory"] : sourceDirectory;
            outputDirectory = options.ContainsKey("outputDirectory") ? options["outputDirectory"] : outputDirectory;
            luac = options.ContainsKey("luac");

            postBuildCommand = options.ContainsKey("postbuild") ? options["postbuild"] : postBuildCommand;
            failBuildCommand = options.ContainsKey("failBuild") ? options["failBuild"] : failBuildCommand;

            isGithub = !options.ContainsKey("no-github");
            pulls = !options.ContainsKey("no-pull");
            queue = !options.ContainsKey("no-queue");
            inQueue = false;


            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("{0}://{1}:{2}/", protocol, domain, port));
            listener.Start();
            _ = HandleRequests();

            Thread.Sleep(-1);
        }

        private async Task HandleRequests()
        {
            Console.WriteLine("Started listening on {0}", string.Format("{0}://{1}:{2}/", protocol, domain, port));
            while (true)
            {
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => {
                    Console.WriteLine("Request received");
                    HandleRequest(context);
                });
            }
        }

        private async void HandleRequest(HttpListenerContext context)
        {
            int statusCode = 200;
            string responseBody = "";
            try
            {
                if (isGithub)
                {
                    StreamReader reader = new StreamReader(context.Request.InputStream);
                    string requestBody = (await reader.ReadToEndAsync());

                    JsonDocument jsonPush = JsonDocument.Parse(requestBody);
                    string pushref = jsonPush.RootElement.GetProperty("ref").GetString();
                    var repository = jsonPush.RootElement.GetProperty("repository");
                    string reponame = repository.GetProperty("name").GetString();

                    Console.WriteLine("Push on repo {0}, ref: {1}", reponame, pushref);

                    if (this.repo != null && reponame != this.repo)
                    {
                        statusCode = 204;
                        responseBody = string.Format("Repo does not match target repo '{0}'", this.repo); 
                    } else if (pushref != this.branchRef)
                    {
                        statusCode = 204;
                        responseBody = string.Format("Ref does not match target ref '{0}'", this.branchRef);
                    } else
                    {
                        Build(ref statusCode, ref responseBody);
                    }
                } else
                {
                    Build(ref statusCode, ref responseBody);
                }

            } catch (Exception e)
            {
                statusCode = 500;
                responseBody = e.Message;
            }


            StreamWriter writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(responseBody);

            context.Response.StatusCode = statusCode;
            writer.Close();
            context.Response.Close();
        }

        private void Build(ref int statusCode, ref string responseBody)
        {

            if (0 == Interlocked.CompareExchange(ref state, 1, 0))
            {
                _ = Task.Run(() =>
                {
                    BuildProject();
                });
                statusCode = 202;
                responseBody = "Build started";
            }
            else
            {
                if (queue)
                {
                    statusCode = 201;
                } else
                {
                    statusCode = 429;
                }
                responseBody = "The server is currently busy building the project";
                if (queue)
                {
                    inQueue = true;
                }
            }
        }

        private async void BuildProject()
        {
            try
            {
                if (pulls)
                {
                    await RunCommand("git", "pull", this.sourceDirectory);
                }
                Console.WriteLine("Build started");
                string arguments = string.Format("build {0} {1}", this.outputDirectory != null ? "-directory \"" + this.outputDirectory + "\"" : "", this.luac ? "-luac" : "");
                Tuple<int, string> result = await RunCommand("slipe", arguments, this.sourceDirectory);
                if (result.Item1 == 0)
                {
                    Console.WriteLine("Build completed");
                    if (postBuildCommand != null)
                    {
                        string[] splits = postBuildCommand.Split(" ");
                        string command = splits[0];
                        string args = splits.Length > 1 ? string.Join(" ", splits.Skip(1)) : "";

                        await RunCommand(command, args, this.sourceDirectory);
                    }
                } else
                {
                    Console.WriteLine("Build completed with non 0 exit code {0}\n{1}", result.Item1, result.Item2);
                    if (failBuildCommand != null)
                    {
                        string[] splits = failBuildCommand.Split(" ");
                        string command = splits[0];
                        string args = splits.Length > 1 ? string.Join(" ", splits.Skip(1)) : "";

                        await RunCommand(command, args, this.sourceDirectory);
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            state = 0;
            if (inQueue)
            {
                inQueue = false;
                BuildProject();
            }
        }

        private async Task<Tuple<int, string>> RunCommand(string command, string arguments = "", string directory = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = directory
            };

            Process process = new Process() { StartInfo = startInfo, };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync() + await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            return new Tuple<int, string>(process.ExitCode, output);
        }
    }
}
