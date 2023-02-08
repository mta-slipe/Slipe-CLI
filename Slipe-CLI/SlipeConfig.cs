using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Buffers;
using Newtonsoft.Json;

namespace Slipe
{
    class ConfigHelper
    {
        public static void Write(SlipeConfig config, string path = "./.slipe")
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            }));
        }

        public static void WriteModule(SlipeModule module, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(module, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            }));
        }

        public static SlipeConfig Read(string path = "./.slipe")
        {
            return JsonConvert.DeserializeObject<SlipeConfig>(File.ReadAllText(path));
        }

        public static SlipeModule ReadModule(string path)
        {
            return JsonConvert.DeserializeObject<SlipeModule>(File.ReadAllText(path));
        }
    }

    class SlipeConfig
    {
        public SlipeConfigCompileTargetList compileTargets;

        public List<string> dlls = new List<string>();
        public List<string> systemComponents = new List<string>();
        public List<SlipeAssetDirectory> assetDirectories = new List<SlipeAssetDirectory>();
        public List<SlipeHttpDirectory> httpDirectories = new List<SlipeHttpDirectory>();
        public List<SlipeModule> modules = new List<SlipeModule>();
        public List<SlipeExport> exports = new List<SlipeExport>();
        public string clientMinVersion;
        public string serverMinVersion;

        public string defaultHttpFile = "";

    }

    class SlipeConfigCompileTargetList
    {
        public List<SlipeConfigCompileTarget> client = new List<SlipeConfigCompileTarget>();
        public List<SlipeConfigCompileTarget> server = new List<SlipeConfigCompileTarget>();
    }

    class SlipeConfigCompileTarget
    {
        public string path;
        public bool? BlockEntryPoint;

        public static implicit operator SlipeConfigCompileTarget(string path)
        {
            return new SlipeConfigCompileTarget()
            {
                path = path,
            };
        }

        public static implicit operator string(SlipeConfigCompileTarget path)
        {
            return path.path;
        }

    }

    class SlipeModule
    {
        public string type;
        public string name;
        public string path;

        public SlipeConfigCompileTargetList compileTargets = new SlipeConfigCompileTargetList();

        public List<string> dlls = new List<string>();
        public List<string> attributes = new List<string>();
        public List<string> systemComponents = new List<string>();
        public List<string> backingLua = new List<string>();
        public List<SlipeAssetDirectory> assetDirectories = new List<SlipeAssetDirectory>();

        public List<SlipeHttpDirectory> httpDirectories = new List<SlipeHttpDirectory>();
    }

    class SlipeAssetDirectory
    {
        public string path;
        public bool downloads;
        public string extension;
    }

    class SlipeExport
    {
        public string name;
        public string niceName;
        public string type;
        public bool isHttp;
    }

    class SlipeHttpDirectory
    {
        public string path;
        public List<string> interpretedFiles = new List<string>();
    }
}
