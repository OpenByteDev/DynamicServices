using DynamicServices.Exceptions;
using NetMQ;
using NetMQ.Sockets;
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

        protected override void RegisterService(byte[] serviceSignature, byte[] methodSignature, ServiceMethod serviceMethod) {
            base.RegisterService(serviceSignature, methodSignature, serviceMethod);
            ((SubscriberSocket)Socket).Subscribe(serviceSignature);
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
            if (TryReceiveInvocation(out byte[] service, out byte[] method, out object[] arguments))
                InvokeServiceMethod(service, method, arguments);
        }
    }
}
