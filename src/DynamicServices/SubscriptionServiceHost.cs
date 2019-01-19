using DynamicServices.Exceptions;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Generic;
using static DynamicServices.ServiceMethod;

namespace DynamicServices {
    public class SubscriptionServiceHost : ServiceHostBase {
        
        public SubscriptionServiceHost() : base(new SubscriberSocket()) {
            Poller.Add(Socket);
            Socket.ReceiveReady += Socket_ReceiveReady;
        }
        public SubscriptionServiceHost(string connectionString) : this() { Connect(connectionString); }
        public SubscriptionServiceHost(string scheme, string host, int port) : this() { Connect(scheme, host, port); }
        public SubscriptionServiceHost(string host, int port) : this() { Connect(host, port); }

        protected override void CheckServiceMethod(in ServiceMethod method) {
            base.CheckServiceMethod(method);
            if (method.ResponseType != MethodResponseType.None)
                throw new ResponseTypeNotSupportedException(method.ResponseType, @"Subscription service methods must return void.");
        }

        protected override void RegisterService(byte[] signature, IDictionary<byte[], ServiceMethod> methods) {
            base.RegisterService(signature, methods);
            ((SubscriberSocket)Socket).Subscribe(signature);
        }

        protected override bool UnregisterService(byte[] service) {
            if (base.UnregisterService(service)) {
                ((SubscriberSocket)Socket).Unsubscribe(service);
                return true;
            }
            return false;
        }

        private void Socket_ReceiveReady(object sender, NetMQSocketEventArgs e) => SocketReceiveReady();
        protected void SocketReceiveReady() {
            if (!TryReceiveMultipartBytes(out List<byte[]> frames, 2))
                return;
            var request = MessagePackSerializer.Deserialize<InvocationRequest>(frames[1]);

            if (GetServiceMethod(request) is ServiceMethod method)
                method.Invoke(request.Arguments);
            else HandleError(new MethodNotRegisteredException());
        }

    }
}
