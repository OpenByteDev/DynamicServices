using System;

namespace DynamicServices {
    public static class IServiceHostExtensions {

        public static void RegisterService<T>(this IServiceHost serviceHost) where T : class, new() => serviceHost?.RegisterService(new T(), typeof(T));
        public static void RegisterService<T, TInterface>(this IServiceHost serviceHost) where T : class, new() where TInterface : class => serviceHost?.RegisterService(new T(), new Type[] { typeof(TInterface) });
        public static void RegisterService<T>(this IServiceHost serviceHost, params Type[] interfaces) where T : class, new() => serviceHost?.RegisterService(new T(), interfaces);
        public static void RegisterService<T>(this IServiceHost serviceHost, T service) => serviceHost?.RegisterService(service, typeof(T));
        public static void RegisterService(this IServiceHost serviceHost, object service) => serviceHost?.RegisterService(service, service?.GetType());
        public static void RegisterService(this IServiceHost serviceHost, object service, Type type) => serviceHost?.RegisterService(service, type?.GetInterfaces());
        public static void RegisterService(this IServiceHost serviceHost, Type serviceType, params object[] args) => serviceHost?.RegisterService(Activator.CreateInstance(serviceType, args), serviceType);

        public static bool UnregisterService(this IServiceHost serviceHost, params Type[] services) {
            if (serviceHost is null)
                return false;
            bool success = true;
            foreach (var service in services)
                if (!serviceHost.UnregisterService(service))
                    success = false;
            return success;
        }
        public static bool UnregisterService<T>(this IServiceHost serviceHost) => serviceHost?.UnregisterService(typeof(T)) ?? false;

    }
}
