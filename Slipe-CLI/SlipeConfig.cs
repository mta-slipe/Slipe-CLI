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
        public static void Write(SlipeConfig config)
        {
            File.WriteAllText("./.slipe", JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            }));
        }

        public static SlipeConfig Read()
        {
            return JsonConvert.DeserializeObject<SlipeConfig>(File.ReadAllText("./.slipe"));
        }
    }

    struct SlipeConfig
    {
        public SlipeConfigCompileTarget compileTargets;

        public List<string> dlls;
        public List<string> systemComponents;
    }

    struct SlipeConfigCompileTarget
    {
        public List<string> client;
        public List<string> server;
    }
}
