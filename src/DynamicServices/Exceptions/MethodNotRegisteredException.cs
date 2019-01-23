using System;

namespace OpenByte.DynamicServices.Exceptions {
    [Serializable]
    public class MethodNotRegisteredException : InvalidOperationException {

        private const string _DefaultMessage = @"The requested method is not registered on the host.";

        public MethodNotRegisteredException() : base(_DefaultMessage) { }
        public MethodNotRegisteredException(string message) : base(message) { }
        public MethodNotRegisteredException(Exception innerException) : base(_DefaultMessage, innerException) { }
        public MethodNotRegisteredException(string message, Exception innerException) : base(message, innerException) { }

    }
}