using Castle.DynamicProxy;
using System;

namespace DynamicServices.Utils {
    internal class SimpleInterceptor : IInterceptor {

        protected readonly Action<IInvocation> Interceptor;

        public SimpleInterceptor(Action<IInvocation> interceptor) {
            Interceptor = interceptor;
        }

        public void Intercept(IInvocation invocation) => Interceptor(invocation);

    }
}
