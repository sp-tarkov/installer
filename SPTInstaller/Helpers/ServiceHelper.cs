using Splat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPTInstaller.Helpers
{
    /// <summary>
    /// A helper class to handle simple service registration to Splat with constructor parameter injection
    /// </summary>
    /// <remarks>Splat only recognizes the registered types and doesn't account for interfaces :(</remarks>
    internal static class ServiceHelper
    {
        private static bool TryRegisterInstance<T, T2>(object[] parameters = null)
        {
            var instance = Activator.CreateInstance(typeof(T2), parameters);

            if (instance != null)
            {
                Locator.CurrentMutable.RegisterConstant<T>((T)instance);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Register a class as a service
        /// </summary>
        /// <typeparam name="T">class to register</typeparam>
        public static void Register<T>() where T : class => Register<T, T>();

        /// <summary>
        /// Register a class as a service by another type
        /// </summary>
        /// <typeparam name="T">type to register as</typeparam>
        /// <typeparam name="T2">class to register</typeparam>
        public static void Register<T, T2>() where T : class
        {
            var constructors = typeof(T2).GetConstructors();

            foreach(var constructor in constructors)
            {
                var parmesan = constructor.GetParameters();

                if(parmesan.Length == 0)
                {
                    if (TryRegisterInstance<T, T2>()) return;

                    continue;
                }

                List<object> parameters = new List<object>();

                for(int i = 0; i < parmesan.Length; i++)
                {
                    var parm = parmesan[i];
                    
                    var parmValue = Get(parm.ParameterType);

                    if (parmValue != null) parameters.Add(parmValue);
                }

                if (TryRegisterInstance<T, T2>(parameters.ToArray())) return;
            }
        }

        /// <summary>
        /// Get a service from splat
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if the service isn't found</exception>
        public static object Get(Type type)
        {
            var service = Locator.Current.GetService(type);

            if (service == null)
                throw new InvalidOperationException($"Could not locate service of type '{type.Name}'");

            return service;
        }

        /// <summary>
        /// Get a service from splat
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if the service isn't found</exception>
        public static T Get<T>()
        {
            var service = Locator.Current.GetService<T>();

            if (service == null)
                throw new InvalidOperationException($"Could not locate service of type '{nameof(T)}'");

            return service;
        }

        /// <summary>
        /// Get all services of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">thrown if no services are found</exception>
        public static T[] GetAll<T>()
        {
            var services = Locator.Current.GetServices<T>().ToArray();

            if (services == null || services.Count() == 0)
                throw new InvalidOperationException($"Could not locate service of type '{nameof(T)}'");

            return services;
        }
    }
}
