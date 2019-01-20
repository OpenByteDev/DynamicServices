using DynamicServices.Exceptions;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading.Tasks;
using static DynamicServices.ServiceMethod;

namespace DynamicServices {
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

            var result = InvoceServiceMethod(serviceMethod, arguments);

            if (!(result is null))
                TrySendMultipartBytes(connection, MessagePackSerializer.Typeless.Serialize(result));
        }

        private object InvoceServiceMethod(ServiceMethod serviceMethod, object[] arguments) {
            switch (serviceMethod.ResponseType) {
                case MethodResponseType.None:
                    return null;
                case MethodResponseType.Sync:
                    return serviceMethod.Invoke(arguments);
                case MethodResponseType.Async:
                    var task = Task.Run(async () => await (Task)serviceMethod.Invoke(arguments));
                    if (task.Wait(InvocationTimeout))
                        return null;
                    else {
                        HandleError(task.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                        return null;
                    }
                case MethodResponseType.AsyncWithResult:
                    var taskWithResult = Task.Run(async () => await (Task<object>)serviceMethod.Invoke(arguments));
                    if (taskWithResult.Wait(InvocationTimeout))
                        return taskWithResult.Result;
                    else {
                        HandleError(taskWithResult.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                        return null;
                    }
                default:
                    HandleError(new ResponseTypeNotSupportedException(serviceMethod.ResponseType));
                    return null;
            }
        }

    }
}
