using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BotevBotApp
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Initializes the default modules.
        /// </summary>
        /// <returns>A task representing the operation.</returns>
        Task InitializeModulesAsync();

        /// <summary>
        /// Initializes the provided modules.
        /// </summary>
        /// <param name="modules">An enumerable containing the modules to be initialized.</param>
        /// <returns>A task representing the operation.</returns>
        Task InitializeModulesAsync(IEnumerable<Type> modules);

        /// <summary>
        /// Initializes all the discovered modules in the provided assemblies.
        /// </summary>
        /// <param name="assemblies">An enumebrable containing the assemblies which are to be searched for modules.</param>
        /// <returns>A task representing the operation.</returns>
        Task InitializeModulesAsync(IEnumerable<Assembly> assemblies);
    }
}