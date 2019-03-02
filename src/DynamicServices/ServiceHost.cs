using OpenByte.DynamicServices.Exceptions;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static OpenByte.DynamicServices.ServiceMethod;

namespace OpenByte.DynamicServices {
    public class ServiceHost : ServiceHostBase {

        public TimeSpan InvocationTimeout = DynamicServicesConfig.DefaultInvocationTimeout;

        public ServiceHost() : base(new RouterSocket()) {
            Poller.Add(Socket);
            Socket.ReceiveReady += Socket_ReceiveReady;
        }
        public ServiceHost(string connectionString) : this() { Bind(connectionString); }
        public ServiceHost(string scheme, string host, int port) : this() { Bind(scheme, host, port); }
        public ServiceHost(string host, int port) : this() { Bind(host, port); }

        private void Socket_ReceiveReady(object sender, NetMQSocketEventArgs e) => SocketReceiveReady();
        protected void SocketReceiveReady() {
            if (!TryReceiveFrameBytes(out byte[] connection) ||
                !TryReceiveInvocation(out byte[] service, out byte[] method, out object[] arguments))
                return;

            if (!(GetServiceMethod(service, method) is ServiceMethod serviceMethod)) {
                HandleError(new MethodNotRegisteredException());
                return;
            }

            var result = TryInvokeServiceMethod(serviceMethod, arguments);

            if (!(result is null))
                TrySendMultipartBytes(connection, MessagePackSerializer.Typeless.Serialize(result));
        }

        protected object TryInvokeServiceMethod(ServiceMethod serviceMethod, object[] arguments) {
            try {
                return InvokeServiceMethod(serviceMethod, arguments);
            } catch (Exception e) {
                HandleError(e);
                return null;
            }
        }

        protected object InvokeServiceMethod(ServiceMethod serviceMethod, object[] arguments) {
            switch (serviceMethod.ResponseType) {
                case MethodResponseType.None:
                    return null;
                case MethodResponseType.Sync:
                    return serviceMethod.Invoke(arguments);
                case MethodResponseType.Async:
                    var task = Task.Run(() => (Task)serviceMethod.Invoke(arguments));
                    if (task.Wait(InvocationTimeout))
                        return 0;
                    else {
                        HandleError(task.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                        return null;
                    }
                case MethodResponseType.AsyncWithResult:
                    return _InvokeServiceMethodAsyncWithResult
                        .MakeGenericMethod(serviceMethod.ReturnType.GetGenericArguments())
                        .Invoke(this, new object[] { serviceMethod, arguments });
                default:
                    HandleError(new ResponseTypeNotSupportedException(serviceMethod.ResponseType));
                    return null;
            }
        }

        private static readonly MethodInfo _InvokeServiceMethodAsyncWithResult =
            typeof(ServiceHost).GetMethod(nameof(InvokeServiceMethodAsyncWithResult), BindingFlags.NonPublic | BindingFlags.Instance);
        private T InvokeServiceMethodAsyncWithResult<T>(ServiceMethod serviceMethod, object[] arguments) {
            var taskWithResult = Task.Run(new Func<Task<T>>(() => (Task<T>)serviceMethod.Invoke(arguments)));
            if (taskWithResult.Wait(InvocationTimeout))
                return taskWithResult.Result;
            else {
                HandleError(taskWithResult.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                return default;
            }
        }

    }
}

