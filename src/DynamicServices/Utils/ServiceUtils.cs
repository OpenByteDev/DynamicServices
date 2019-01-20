using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using MessagePack;

namespace DynamicServices.Utils {
    internal static class ServiceUtils {

        private static readonly HashAlgorithm _HashAlgorithm = MD5.Create();
        private static readonly Encoding _Encoding = Encoding.UTF8;
        private static readonly ConcurrentDictionary<MethodInfo, byte[]> _MethodSignatureCache = new ConcurrentDictionary<MethodInfo, byte[]>();

        public static byte[] GetMethodSignature(MethodInfo methodInfo) {
            return _MethodSignatureCache.GetOrAdd(methodInfo, GenerateMethodSignature);
        }
        private static byte[] GenerateMethodSignature(MethodInfo methodInfo) {
            var signature = new StringBuilder(methodInfo.Name);
            foreach (var parameter in methodInfo.GetParameters())
                signature.Append(parameter.ParameterType.FullName);
            var bytes = _HashAlgorithm.ComputeHash(_Encoding.GetBytes(signature.ToString()));
            return bytes;
        }
        public static byte[] CombineSignatures(byte[] service, byte[] method) {
            byte[] signature = new byte[service.Length + method.Length];
            Buffer.BlockCopy(service, 0, signature, 0, service.Length);
            Buffer.BlockCopy(method, 0, signature, service.Length, method.Length);
            return signature;
        }

        public static byte[] GetTypeSignature(Type type) => type.GUID.ToByteArray();

        public static byte[][] SerializeInvocation(IInvocation invocation) => new byte[][] {
            GetTypeSignature(invocation.Method.DeclaringType),
            GetMethodSignature(invocation.Method),
            SerializeArguments(invocation.Arguments)
        };
        public static void DeserializeInvocation(IList<byte[]> frames, out byte[] service, out byte[] method, out object[] arguments) {
            service = frames[0];
            method = frames[1];
            arguments = DeserializeArguments(frames[2]);
        }

        public static byte[] SerializeArguments(object[] arguments) =>
            MessagePackSerializer.Typeless.Serialize(arguments);
        public static object[] DeserializeArguments(byte[] bytes) =>
            (object[])MessagePackSerializer.Typeless.Deserialize(bytes);

    }
}
