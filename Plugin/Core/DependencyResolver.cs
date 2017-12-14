using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;

namespace AIMP.DiskCover.Core
{
    internal class DependencyResolver : ServiceLocatorImplBase
    {
        private IEnumerable<object> _objects;
        private static DependencyResolver _instance;
        public static DependencyResolver Current => _instance ?? (_instance = new DependencyResolver());

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return key == null
                ? _objects.First(serviceType.IsInstanceOfType)
                : _objects.First(o => serviceType.IsInstanceOfType(o) && Equals(key, o.GetType().FullName));
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _objects.Where(serviceType.IsInstanceOfType);
        }

        public void Register(IEnumerable<object> objects)
        {
            _objects = objects;
        }
    }
}