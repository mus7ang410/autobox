using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autobox.Desktop.Services
{
    public class ServiceProviderException : Exception
    {
        public ServiceProviderException(string reason) :
            base(reason)
        {

        }
    }

    // ### ServiceProvider
    // Provide registered services as singleton
    public static class ServiceProvider
    {
        // ###### AddService
        // Register a service as a singleton
        // Throw a ServiceProviderException if TInterface is already registered
        public static TService AddService<TInterface, TService>(TService service) where TService : TInterface where TInterface : class
        {
            Type key = typeof(TInterface);
            if (Services.ContainsKey(key))
            {
                throw new ServiceProviderException($"Interface {key.Name} is already registered under ServiceProvider");
            }
            Services.Add(key, service);
            return service;
        }

        // ##### GetService
        // Get a registered service
        // Throw a ServiceProviderException if TInterface is not registered
        public static TInterface GetService<TInterface>() where TInterface : class
        {
            Type key = typeof(TInterface);
            if (!Services.ContainsKey(key))
            {
                return null;
            }
            return (TInterface)Services[key];
        }

        // ##### Attributes
        private static Dictionary<Type, object> Services = new Dictionary<Type, object>();
    }
}
