using Castle.DynamicProxy;
using OpenByte.DynamicServices.Utils;
using NetMQ;
using System;

namespace OpenByte.DynamicServices {
    public abstract class ServiceClientBase : ServiceSocketBase, IServiceClient {

        public ServiceClientBase(NetMQSocket socket) : base(socket) { }

        public object GetServiceProxy(Type type, Type[] additionalInterfaces) =>
            // ServiceProxyGenerator2.GetProxy(type, HandleInterception);
            ServiceBuilder.Build(type, additionalInterfaces, new SimpleInterceptor(HandleInvocation));

        protected abstract void HandleInvocation(IInvocation invocation);

    }
}
