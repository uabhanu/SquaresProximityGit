namespace Misc
{
    using System;
    using System.Collections.Generic;
    
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type , object> SERVICES = new();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if(!SERVICES.ContainsKey(type))
            {
                SERVICES[type] = service;
            }
        }

        public static T Get<T>() where T : class
        {
            SERVICES.TryGetValue(typeof(T) , out var service);
            return service as T;
        }

        public static void Clear()
        {
            SERVICES.Clear();
        }
    }
}