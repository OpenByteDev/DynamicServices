using System;
using Castle.DynamicProxy;

namespace DynamicServices {
    public class ServiceBuilder {
        
        private ProxyGenerator _ProxyGenerator;
        public ProxyGenerator ProxyGenerator => _ProxyGenerator ?? (_ProxyGenerator = new ProxyGenerator());
        public ProxyGenerationOptions ProxyGenerationOptions = ProxyGenerationOptions.Default;

        public T Build<T>(IInterceptor interceptor) where T : class => Build(typeof(T), interceptor) as T;
        public object Build(Type type, IInterceptor interceptor) => Build(type, Array.Empty<Type>(), interceptor);
        public object Build(Type type, Type[] additionalInterfaces, IInterceptor interceptor) {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsInterface)
                throw new ArgumentException(@"Service type has to be an interface", nameof(type));
            if (additionalInterfaces is null)
                throw new ArgumentNullException(nameof(additionalInterfaces));
            foreach (var i in additionalInterfaces)
                if (i is null || !i.IsInterface)
                    throw new ArgumentException(@"Additional interface list contains a non interface type", nameof(additionalInterfaces));
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(type, additionalInterfaces, ProxyGenerationOptions, interceptor);
        }

    }
}
