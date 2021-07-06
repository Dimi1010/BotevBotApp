using System;
using System.Collections.Generic;
using System.Reflection;

namespace BotevBotApp
{
    public interface IModuleInfo
    {
        /// <summary>
        /// Gets all the modules containing commands.
        /// </summary>
        /// <returns>An enumerable containing type information for the command modules.</returns>
        public IEnumerable<Type> GetCommandModules();

        /// <summary>
        /// Gets the <see cref="Assembly"/> containing the module.
        /// </summary>
        /// <returns>The containing <see cref="Assembly"/>.</returns>
        public Assembly GetModuleAssembly();
    }
}
