using System;

namespace AIMP.DiskCover.Infrastructure
{
    public abstract class DependencyResolver
    {
        /// <summary>
        ///     The current instance of the <see cref="DependencyResolver" />
        /// </summary>
        public static DependencyResolver Current { get; set; }

        public abstract DependencyResolver GetChildrenContainer { get; }

        /// <summary>
        ///     Resolves the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public abstract object ResolveService(Type serviceType);

        /// <summary>
        ///     Resolves the service.
        /// </summary>
        /// <typeparam name="TServiceType">The type of the service type.</typeparam>
        public abstract TServiceType ResolveService<TServiceType>();
    }
}