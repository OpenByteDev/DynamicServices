using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

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

        public static byte[] GetTypeSignature(Type type) => type.GUID.ToByteArray();

    }
}
