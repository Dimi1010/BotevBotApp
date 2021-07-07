using System;
using System.Collections.Generic;
using System.Reflection;

namespace BotevBotApp
{
    public class CoreModuleInfo : IModuleInfo
    {
        public IEnumerable<Type> GetCommandModules()
        {
            return new List<Type> { typeof(CommandModules.PublicModule), typeof(CommandModules.DebugModule) };
        }

        public Assembly GetModuleAssembly() => Assembly.GetExecutingAssembly();
    }
}
