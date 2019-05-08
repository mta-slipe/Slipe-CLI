﻿using System;
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

    struct SlipeConfig
    {
        public SlipeConfigCompileTarget compileTargets;

        public List<string> dlls;
        public List<string> systemComponents;
        public List<SlipeAssetDirectory> assetDirectories;
        public List<SlipeModule> modules;
        public string clientMinVersion;
        public string serverMinVersion;
    }

    struct SlipeConfigCompileTarget
    {
        public List<string> client;
        public List<string> server;
    }

    struct SlipeModule
    {
        public string type;
        public string name;
        public string path;

        public SlipeConfigCompileTarget compileTargets;

        public List<string> dlls;
        public List<string> attributes;
        public List<string> systemComponents;
        public List<string> backingLua;
        public List<SlipeAssetDirectory> assetDirectories;
    }

    struct SlipeAssetDirectory
    {
        public string path;
        public bool downloads;
    }
}
