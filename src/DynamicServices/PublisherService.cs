using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;

namespace OpenByte.DynamicServices {
    public class PublisherService : ServiceClientBase {

        protected readonly NetMQQueue<IInvocation> Queue;

        public PublisherService() : base(new PublisherSocket()) {
            Queue = new NetMQQueue<IInvocation>();
            Poller.Add(Queue);
            Queue.ReceiveReady += Queue_ReceiveReady;
        }
        public PublisherService(string connectionString) : this() { Bind(connectionString); }
        public PublisherService(string scheme, string host, int port) : this() { Bind(scheme, host, port); }
        public PublisherService(string host, int port) : this() { Bind(host, port); }

        protected override void HandleInvocation(IInvocation invocation) =>
            Queue.Enqueue(invocation);

        private void Queue_ReceiveReady(object sender, NetMQQueueEventArgs<IInvocation> e) =>
            HandleQueueItem(e.Queue.Dequeue());
        protected void HandleQueueItem(IInvocation invocation) =>
            TrySendInvocation(invocation);
            

    }
}
