/*
using Castle.DynamicProxy;
using DynamicServices.Utils;
using MessagePack;
using System;
using System.Linq;

namespace DynamicServices {
    [MessagePackObject]
    public readonly struct InvocationRequest : IEquatable<InvocationRequest> {

        [Key(0)]
        public readonly byte[] Service;
        [Key(1)]
        public readonly byte[] Method;
        [Key(2)]
        public readonly object[] Arguments;

        [SerializationConstructor]
        public InvocationRequest(byte[] service, byte[] method, object[] arguments) =>
            (Service, Method, Arguments) = (service, method, arguments);
        
        public void Deconstruct(out byte[] service, out byte[] method, out object[] arguments) =>
            (service, method, arguments) = (Service, Method, Arguments);
        // public InvocationRequest() : this(Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<object>()) { }
        
        public override bool Equals(object obj) =>
            obj is InvocationRequest invocationRequest && this.Equals(invocationRequest);
        public bool Equals(InvocationRequest other) =>
            Service.SequenceEqual(other.Service) &&
            Method.SequenceEqual(other.Method) &&
            Arguments.SequenceEqual(other.Arguments);
        
        public override int GetHashCode() => (Service, Method, Arguments).GetHashCode();

        public static bool operator ==(InvocationRequest a, InvocationRequest b) => a.Equals(b);
        public static bool operator !=(InvocationRequest a, InvocationRequest b) => !(a == b);

        public static InvocationRequest From(IInvocation invocation) =>
            new InvocationRequest(
                ServiceUtils.GetTypeSignature(invocation.Method.DeclaringType),
                ServiceUtils.GetMethodSignature(invocation.Method),
                invocation.Arguments
            );

    }
}
*/