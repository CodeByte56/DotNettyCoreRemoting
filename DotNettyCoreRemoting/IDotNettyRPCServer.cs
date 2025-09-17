using DotNettyCoreRemoting.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting
{
    public interface IDotNettyCoreServer
    {

        /// <summary>
        /// Gets the dependency injection container that is used a service registry.
        /// </summary>
        IDependencyInjectionContainer ServiceRegistry { get; }

    }
}
