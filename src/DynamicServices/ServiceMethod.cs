using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenByte.DynamicServices {
    public readonly struct ServiceMethod : IEquatable<ServiceMethod> {
        
        public readonly string Name;
        public readonly Type ReturnType;
        public readonly MethodInfo MethodInfo;
        public readonly object Service;
        public readonly MethodResponseType ResponseType;

        public ServiceMethod(MethodInfo methodInfo, object service) {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Name = methodInfo.Name;
            ReturnType = methodInfo.ReturnType;
            ResponseType = GetResponseType(methodInfo);
        }

        public object Invoke(params object[] arguments) => MethodInfo.Invoke(Service, arguments);
        
        public override bool Equals(object obj) =>
            obj is ServiceMethod serviceMethod && Equals(serviceMethod);
        
        public bool Equals(ServiceMethod other) =>
            MethodInfo == other.MethodInfo &&
            Service == other.Service;
        
        public override int GetHashCode() => (MethodInfo, Service).GetHashCode();
        
        public void Deconstruct(out string name, out Type returnType, out MethodInfo methodInfo, out object service, out MethodResponseType responseType) =>
            (name, returnType, methodInfo, service, responseType) = (Name, ReturnType, MethodInfo, Service, ResponseType);

        public static bool operator ==(ServiceMethod a, ServiceMethod b) => a.Equals(b);
        public static bool operator !=(ServiceMethod a, ServiceMethod b) => !(a == b);

        public enum MethodResponseType {
            None,
            Async,
            AsyncWithResult,
            Sync
        }

        public static MethodResponseType GetResponseType(in ServiceMethod serviceMethod) => serviceMethod.ResponseType;
        public static MethodResponseType GetResponseType(IInvocation invocation) => GetResponseType(invocation?.Method);
        public static MethodResponseType GetResponseType(MethodInfo methodInfo) {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));
            var returnType = methodInfo.ReturnType;
            if (returnType == typeof(void))
                return MethodResponseType.None;
            if (returnType == typeof(Task))
                return MethodResponseType.Async;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return MethodResponseType.AsyncWithResult;
            return MethodResponseType.Sync;
        }
    }
}
