using DynamicServices.Exceptions;
using DynamicServices.Utils;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicServices {
    public abstract class ServiceHostBase : ServiceSocketBase, IServiceHost {

        protected readonly IDictionary<byte[], ServiceMethod> Services = new Dictionary<byte[], ServiceMethod>(StructuralEqualityComparer<byte[]>.Default);

        public ServiceHostBase(NetMQSocket socket) : base(socket) { }

        public void RegisterService(object service, Type[] interfaces) {
            if (service is null)
                throw new ArgumentNullException(nameof(service));
            if (interfaces is null)
                throw new ArgumentNullException(nameof(interfaces));
            if (interfaces.Length == 0)
                throw new ArgumentException(@"Service does not implement any interfaces or provided interface list is empty", nameof(service));
            foreach (var i in interfaces)
                RegisterService(service, i);
        }
        protected void RegisterService(object service, Type @interface) {
            var signature = ServiceUtils.GetTypeSignature(@interface);
            CheckServiceType(@interface);
            var methods = @interface.GetMethods();
            if (methods.Length == 0)
                return;

            RegisterService(service, signature, methods);
        }
        protected void RegisterService(object service, byte[] serviceSignature, MethodInfo[] methods) {
            foreach (var method in methods) {
                var methodSignature = ServiceUtils.GetMethodSignature(method);
                var serviceMethod = new ServiceMethod(method, service);
                CheckServiceMethod(serviceMethod);
                RegisterService(serviceSignature, methodSignature, serviceMethod);
            }
        }
        protected virtual void RegisterService(byte[] serviceSignature, byte[] methodSignature, ServiceMethod serviceMethod) =>
            Services.Add(ServiceUtils.CombineSignatures(serviceSignature, methodSignature), serviceMethod);

        protected virtual void CheckServiceType(Type service) { }
        protected virtual void CheckServiceMethod(in ServiceMethod method) {
            var parameters = method.MethodInfo.GetParameters();
            foreach (var parameter in parameters)
                CheckServiceMethodParameter(method, parameter);
        }
        protected virtual void CheckServiceMethodParameter(in ServiceMethod method, ParameterInfo parameter) {
            if (parameter.HasDefaultValue)
                throw new NotSupportedException(@"Service methods cannot have default parameters");
            if (parameter.IsIn || parameter.IsOut || parameter.IsRetval || parameter.IsOptional || parameter.ParameterType.IsByRef)
                throw new NotSupportedException(@"Service methods cannot have in, out, retval, optional or ref parameters");
        }

        public bool UnregisterService(Type service) {
            if (service is null)
                throw new ArgumentNullException(nameof(service));
            if (service.IsInterface)
                return UnregisterService(ServiceUtils.GetTypeSignature(service));
            return this.UnregisterService(service.GetInterfaces());
        }
        protected virtual bool UnregisterService(byte[] service) => Services.Remove(service);

        // protected ServiceMethod? GetServiceMethod(in InvocationRequest request) =>
        //    GetServiceMethod(request.Service, request.Method);

        protected ServiceMethod? GetServiceMethod(byte[] service, byte[] method) =>
            GetServiceMethod(ServiceUtils.CombineSignatures(service, method));
        protected ServiceMethod? GetServiceMethod(byte[] signature) {
            if (Services.TryGetValue(signature, out ServiceMethod serviceMethod))
                return serviceMethod;
            return null;
        }
        protected bool InvokeServiceMethod(byte[] service, byte[] method, object[] arguments, out object result) {
            if (GetServiceMethod(service, method) is ServiceMethod serviceMethod) {
                result = serviceMethod.Invoke(arguments);
                return true;
            }
            result = null;
            HandleError(new MethodNotRegisteredException());
            return false;
        }
        protected bool InvokeServiceMethod(byte[] service, byte[] method, object[] arguments) =>
            InvokeServiceMethod(service, method, arguments, out object result);
    }
}
