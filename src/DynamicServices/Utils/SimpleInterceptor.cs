using Castle.DynamicProxy;
using System;

namespace OpenByte.DynamicServices.Utils {
    internal class SimpleInterceptor : IInterceptor {

        protected readonly Action<IInvocation> Interceptor;

        public SimpleInterceptor(Action<IInvocation> interceptor) {
            Interceptor = interceptor;
        }

        public void Intercept(IInvocation invocation) => Interceptor(invocation);

    }
}
