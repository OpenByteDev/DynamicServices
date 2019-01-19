/*
using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace DynamicServices.Utils {
    internal class SimpleInterceptorSelector : IInterceptorSelector {

        protected readonly Func<Type, MethodInfo, IInterceptor[], IInterceptor[]> InterceptorSelector;

        public SimpleInterceptorSelector(Func<Type, MethodInfo, IInterceptor[], IInterceptor[]> interceptorSelector) {
            InterceptorSelector = interceptorSelector;
        }

        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors) => InterceptorSelector(type, method, interceptors);

    }
}
*/
