using DynamicServices.Exceptions;
using MessagePack;
using MessagePack.Resolvers;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
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
            if (!TryReceiveMultipartBytes(out List<byte[]> frames, 2))
                return;

            var connection = frames[0];
            var message = frames[1];
            var (service, method, arguments) = MessagePackSerializer.Deserialize<InvocationRequest>(message);
            if (!(GetServiceMethod(service, method) is ServiceMethod serviceMethod)) {
                HandleError(new MethodNotRegisteredException());
                return;
            }

            var result = serviceMethod.Invoke(arguments);
            object _result = null;

            switch (GetResponseType(serviceMethod)) {
                case MethodResponseType.None:
                    return;
                case MethodResponseType.Async:
                    var task = result as Task;
                    if (task.Status == TaskStatus.RanToCompletion)
                        break;
                    else if (task.IsFaulted) {
                        HandleError(task.Exception);
                        return;
                    }
                    else if (task.Wait(InvocationTimeout))
                        break;
                    else {
                        HandleError(task.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                        return;
                    }
                case MethodResponseType.AsyncWithResult:
                    var taskWithResult = result as Task<object>;
                    if (taskWithResult.Status == TaskStatus.RanToCompletion)
                        _result = taskWithResult.Result;
                    else if (taskWithResult.IsFaulted) {
                        HandleError(taskWithResult.Exception);
                        return;
                    } else if (taskWithResult.Wait(InvocationTimeout))
                        _result = taskWithResult.Result;
                    else {
                        HandleError(taskWithResult.Exception as Exception ?? new InvocationTimeoutException(InvocationTimeout, serviceMethod));
                        return;
                    }
                    break;
                case MethodResponseType.Sync:
                    _result = result;
                    break;
                case var responseType:
                    HandleError(new ResponseTypeNotSupportedException(responseType));
                    return;
            }

            TrySendMultipartBytes(connection, MessagePackSerializer.Typeless.Serialize(_result));
        }

    }
}
