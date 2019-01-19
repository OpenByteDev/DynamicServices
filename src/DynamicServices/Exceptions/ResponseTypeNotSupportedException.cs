using System;
using static DynamicServices.ServiceMethod;

namespace DynamicServices.Exceptions {
    [Serializable]
    public class ResponseTypeNotSupportedException : NotSupportedException {

        private const string _DefaultMessage = @"ResponseType is not supported.";

        public MethodResponseType ResponseType;

        public ResponseTypeNotSupportedException() : this(_DefaultMessage) { }
        public ResponseTypeNotSupportedException(string message) : base(message) { }
        public ResponseTypeNotSupportedException(Exception innerException) : this(_DefaultMessage, innerException) { }
        public ResponseTypeNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
        public ResponseTypeNotSupportedException(MethodResponseType responseType) : this(responseType, $@"ResponseType {responseType} is not supported.") { }
        public ResponseTypeNotSupportedException(MethodResponseType responseType, string message) : this(message) {
            ResponseType = responseType;
        }
        public ResponseTypeNotSupportedException(MethodResponseType responseType, Exception innerException) : this(responseType, $@"ResponseType {responseType} is not supported.", innerException) { }
        public ResponseTypeNotSupportedException(MethodResponseType responseType, string message, Exception innerException) : this(message, innerException) {
            ResponseType = responseType;
        }


    }
}