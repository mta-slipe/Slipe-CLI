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
        public SlipeConfigCompileTarget compileTargets;

        public List<string> dlls = new List<string>();
        public List<string> systemComponents = new List<string>();
        public List<SlipeAssetDirectory> assetDirectories = new List<SlipeAssetDirectory>();
        public List<SlipeModule> modules = new List<SlipeModule>();
        public string clientMinVersion;
        public string serverMinVersion;
    }

    class SlipeConfigCompileTarget
    {
        public List<string> client = new List<string>();
        public List<string> server = new List<string>();
    }

    class SlipeModule
    {
        public string type;
        public string name;
        public string path;

        public SlipeConfigCompileTarget compileTargets;

        public List<string> dlls = new List<string>();
        public List<string> attributes = new List<string>();
        public List<string> systemComponents = new List<string>();
        public List<string> backingLua = new List<string>();
        public List<SlipeAssetDirectory> assetDirectories = new List<SlipeAssetDirectory>();
    }

    class SlipeAssetDirectory
    {
        public string path;
        public bool downloads;
        public string extension;
    }
}
