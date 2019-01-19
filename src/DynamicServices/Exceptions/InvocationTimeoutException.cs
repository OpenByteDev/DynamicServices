using Castle.DynamicProxy;
using System;
using System.Reflection;

namespace DynamicServices.Exceptions {
    [Serializable]
    public class InvocationTimeoutException : TimeoutException {

        public TimeSpan Timeout;
        public Type Service;
        public MethodInfo Method;

        public InvocationTimeoutException(TimeSpan timeout, Type service, MethodInfo method) : base($@"Invocation timeout occured during execution of {service.FullName}.{method.Name}.") {
            Timeout = timeout;
            Service = service;
            Method = method;
        }
        public InvocationTimeoutException(TimeSpan timeout, ServiceMethod serviceMethod) : this(timeout, serviceMethod.MethodInfo.DeclaringType, serviceMethod.MethodInfo) { }
        public InvocationTimeoutException(TimeSpan timeout, IInvocation invocation) : this(timeout, invocation.Method.DeclaringType, invocation.Method) { }
        public InvocationTimeoutException(TimeSpan timeout, MethodInfo methodInfo) : this(timeout, methodInfo.DeclaringType, methodInfo) { }

    }
}
