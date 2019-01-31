using System;

namespace OpenByte.DynamicServices {
    public static class ServiceClientExtensions {

        public static T GetServiceProxy<T>(this IServiceClient serviceClient) where T : class => serviceClient?.GetServiceProxy(typeof(T)) as T;
        public static T GetServiceProxy<T>(this IServiceClient serviceClient, params Type[] additionalInterfaces) where T : class => serviceClient?.GetServiceProxy(typeof(T), additionalInterfaces) as T;
        public static object GetServiceProxy(this IServiceClient serviceClient, Type type, params Type[] additionalInterfaces) => serviceClient?.GetServiceProxy(type, additionalInterfaces);

    }
}
