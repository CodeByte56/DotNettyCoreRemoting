using System;

namespace DotNettyCoreRemoting.RemoteDelegates
{
    /// <summary>
    /// Interface to be implemented by delegate proxies.
    /// </summary>
    public interface IDelegateProxy : IDisposable
    {
        /// <summary>
        /// Gets the proxied delegate.
        /// </summary>
        Delegate ProxiedDelegate { get; }
    }
}