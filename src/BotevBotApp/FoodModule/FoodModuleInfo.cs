using System;
using System.Collections.Generic;
using System.Reflection;

namespace BotevBotApp.FoodModule
{
    public class FoodModuleInfo : IModuleInfo
    {
        public IEnumerable<Type> GetCommandModules() => new List<Type> { typeof(FoodModuleCommands) };

        public Assembly GetModuleAssembly() => Assembly.GetExecutingAssembly();
    }
}
