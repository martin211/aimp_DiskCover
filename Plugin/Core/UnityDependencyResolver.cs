using System;
using Unity;

namespace AIMP.DiskCover.Core
{
    public class UnityDependencyResolver : DependencyResolver
    {
        private readonly IUnityContainer _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnityDependencyResolver" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public UnityDependencyResolver(IUnityContainer container)
        {
            _container = container;
        }

        public override DependencyResolver GetChildrenContainer => new UnityDependencyResolver(_container
            .CreateChildContainer());

        /// <summary>
        ///     Resolves the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override object ResolveService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public override TServiceType ResolveService<TServiceType>()
        {
            return _container.Resolve<TServiceType>();
        }
    }
}