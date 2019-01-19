using System;

namespace DynamicServices.Exceptions {
    [Serializable]
    public class SendTimeoutException : TimeoutException {

        private const string _DefaultMessage = @"Timeout while sending";

        public SendTimeoutException() : this(_DefaultMessage) { }
        public SendTimeoutException(string message) : base(message) { }
        public SendTimeoutException(Exception innerException) : this(_DefaultMessage, innerException) { }
        public SendTimeoutException(string message, Exception innerException) : base(message, innerException) { }

    }
}
