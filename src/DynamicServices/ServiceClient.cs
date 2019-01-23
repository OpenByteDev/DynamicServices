using Castle.DynamicProxy;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using OpenByte.DynamicServices.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using static OpenByte.DynamicServices.ServiceMethod;

namespace OpenByte.DynamicServices {
    public class ServiceClient : ServiceClientBase {

        public TimeSpan InvocationTimeout = DynamicServicesConfig.DefaultInvocationTimeout;

        protected readonly NetMQQueue<(IInvocation, TaskCompletionSource<object>)> Queue;

        public ServiceClient() : base(new DealerSocket()) {
            Queue = new NetMQQueue<(IInvocation, TaskCompletionSource<object>)>();
            Poller.Add(Queue);
            Queue.ReceiveReady += Queue_ReceiveReady;
        }
        public ServiceClient(string connectionString) : this() { Connect(connectionString); }
        public ServiceClient(string scheme, string host, int port) : this() { Connect(scheme, host, port); }
        public ServiceClient(string host, int port) : this() { Connect(host, port); }

        protected override void HandleInvocation(IInvocation invocation) {
            var responseType = ServiceMethod.GetResponseType(invocation);

            var source = responseType != MethodResponseType.None ? new TaskCompletionSource<object>() : null;
            Queue.Enqueue((invocation, source));

            switch (responseType) {
                case MethodResponseType.None:
                    return;
                case MethodResponseType.Async:
                case MethodResponseType.AsyncWithResult:
                    var cancellationSource = new CancellationTokenSource(InvocationTimeout);
                    cancellationSource.Token.Register(() => {
                        source.TrySetException(new InvocationTimeoutException(InvocationTimeout, invocation));
                        cancellationSource.Dispose();
                    }, useSynchronizationContext: false);
                    source.Task.ContinueWith(_ => cancellationSource.Cancel(), TaskContinuationOptions.NotOnCanceled);
                    invocation.ReturnValue = source.Task;
                    break;
                case MethodResponseType.Sync:
                    if (source.Task.Wait(InvocationTimeout))
                        invocation.ReturnValue = source.Task.Result;
                    else {
                        source.TrySetCanceled();
                        HandleError(source.Task.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, invocation));
                    }
                    break;
                default:
                    HandleError(new ResponseTypeNotSupportedException(responseType));
                    break;
            }
        }
        
        private void Queue_ReceiveReady(object sender, NetMQQueueEventArgs<(IInvocation, TaskCompletionSource<object>)> e) =>
            HandleQueueItem(e.Queue.Dequeue());
        // private static readonly MethodInfo _Deserializer = typeof(MessagePackSerializer).GetMethod("Deserialize", new Type[] { typeof(byte[]) });
        protected void HandleQueueItem(in (IInvocation, TaskCompletionSource<object>) e) {
            var (invocation, source) = e;

            if (!TrySendInvocation(invocation, source))
                return;

            if (source is null || source.Task.IsCompleted)
                return;

            if (!TryReceiveFrameBytes(out byte[] bytes, source))
                return;

            // var result = _Deserializer.MakeGenericMethod(invocation.Method.ReturnType).Invoke(null, new object[] { bytes });
            var result = MessagePackSerializer.Typeless.Deserialize(bytes);
            source.TrySetResult(result);
        }

    }
}
