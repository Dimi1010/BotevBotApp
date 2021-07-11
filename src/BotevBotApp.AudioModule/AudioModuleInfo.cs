using System;
using System.Collections.Generic;
using System.Reflection;

namespace BotevBotApp.AudioModule
{
    public class AudioModuleInfo : IModuleInfo
    {
        public IEnumerable<Type> GetCommandModules() => new List<Type> { typeof(AudioModuleCommands) };

        public Assembly GetModuleAssembly() => Assembly.GetExecutingAssembly();
    }
}
