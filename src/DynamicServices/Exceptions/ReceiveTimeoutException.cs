using System;

namespace DynamicServices.Exceptions {
    [Serializable]
    public class ReceiveTimeoutException : TimeoutException {

        private const string _DefaultMessage = @"Timeout during receive";

        public ReceiveTimeoutException() : this(_DefaultMessage) { }
        public ReceiveTimeoutException(string message) : base(message) { }
        public ReceiveTimeoutException(Exception innerException) : this(_DefaultMessage, innerException) { }
        public ReceiveTimeoutException(string message, Exception innerException) : base(message, innerException) { }

    }
}
