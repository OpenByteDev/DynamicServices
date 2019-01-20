using Castle.DynamicProxy;
using DynamicServices.Exceptions;
using DynamicServices.Utils;
using MessagePack;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicServices {
    public abstract class ServiceSocketBase : IDisposable {

        protected const string DefaultProtocol = @"tcp";

        protected readonly NetMQSocket Socket;
        protected readonly ServiceBuilder ServiceBuilder;
        protected readonly NetMQPoller Poller;
        protected readonly List<string> EndPoints;

        public TimeSpan SendReceiveTimeout = DynamicServicesConfig.DefaultSendReceiveTimeout;
        public ErrorAction ErrorAction = DynamicServicesConfig.DefaultErrorAction;
        public bool IsRunning { get; protected set; }
        public bool IsStopped => !IsRunning;

        protected ServiceSocketBase(NetMQSocket socket) {
            Socket = socket;
            ServiceBuilder = new ServiceBuilder();
            Poller = new NetMQPoller();
            EndPoints = new List<string>();
        }

        public void Start() {
            if (IsRunning)
                throw new InvalidOperationException(@"Service Socket is already running");
            _Start();
        }
        public bool TryStart() {
            if (IsRunning)
                return false;
            _Start();
            return true;
        }
        protected virtual void _Start() {
            Poller.RunAsync();
            IsRunning = true;
        }
        public void Stop() {
            if (!IsRunning)
                throw new InvalidOperationException(@"Service Socket is not running");
            _Stop();
        }
        public bool TryStop() {
            if (!IsRunning)
                return false;
            _Stop();
            return true;
        }
        protected virtual void _Stop() {
            Poller.StopAsync();
            IsRunning = false;
        }


        #region Bind,Unbind,Connect,Disconnect
        public void Bind(string address) {
            Socket.Bind(address);
            EndPoints.Add(address);
        }
        public void Bind(string host, int port) => Bind(GetConnectionString(host, port));
        public void Bind(string scheme, string host, int port) => Bind(GetConnectionString(scheme, host, port));

        public int BindRandomPort(string host) {
            var address = $@"{DefaultProtocol}://{host}";
            var port = Socket.BindRandomPort(address);
            EndPoints.Add($@"{address}:{port}");
            return port;
        }

        public void Unbind(string address) {
            Socket.Unbind(address);
            EndPoints.Remove(address);
        }
        public void Unbind(string host, int port) => Unbind(GetConnectionString(host, port));
        public void Unbind(string scheme, string host, int port) => Unbind(GetConnectionString(scheme, host, port));
        public void UnbindAll() {
            foreach (var endpoint in EndPoints.Copy())
                Unbind(endpoint);
        }

        public void Connect(string address) {
            Socket.Connect(address);
            EndPoints.Add(address);
        }
        public void Connect(string host, int port) => Connect(GetConnectionString(host, port));
        public void Connect(string scheme, string host, int port) => Connect(GetConnectionString(scheme, host, port));

        public void Disconnect(string address) {
            Socket.Disconnect(address);
            EndPoints.Remove(address);
        }
        public void Disconnect(string host, int port) => Disconnect(GetConnectionString(host, port));
        public void Disconnect(string scheme, string host, int port) => Disconnect(GetConnectionString(scheme, host, port));
        public void DisconnectAll() {
            foreach (var endpoint in EndPoints)
                Disconnect(endpoint);
        }
        #endregion

        public void Shutdown() {
            Stop();
            UnbindAll();
        }
        public bool TryShutdown() {
            if (TryStop()) {
                UnbindAll();
                return true;
            }
            return false;
        }

        protected static string GetConnectionString(string scheme, string host, int port) => $@"{scheme}://{host}:{port}";
        protected static string GetConnectionString(string host, int port) => GetConnectionString(DefaultProtocol, host, port);

        #region Send & Receive
        protected bool TryReceiveMultipartBytes(out List<byte[]> frames, int expectedFrameCount) {
            frames = new List<byte[]>();
            if (Socket.TryReceiveMultipartBytes(SendReceiveTimeout, ref frames, expectedFrameCount))
                return true;
            else {
                HandleError(new ReceiveTimeoutException());
                return false;
            }
        }
        protected bool TryReceiveMultipartBytes<T>(out List<byte[]> frames, int expectedFrameCount, TaskCompletionSource<T> completionSource) {
            frames = new List<byte[]>();
            if (Socket.TryReceiveMultipartBytes(SendReceiveTimeout, ref frames, expectedFrameCount))
                return true;
            else {
                HandleError(new SendTimeoutException(), completionSource);
                return false;
            }
        }
        protected bool TryReceiveFrameBytes(out byte[] frame) {
            if (Socket.TryReceiveFrameBytes(SendReceiveTimeout, out frame))
                return true;
            else {
                HandleError(new ReceiveTimeoutException());
                return false;
            }
        }
        protected bool TryReceiveFrameBytes<T>(out byte[] frame, TaskCompletionSource<T> completionSource) {
            if (Socket.TryReceiveFrameBytes(SendReceiveTimeout, out frame))
                return true;
            else {
                HandleError(new SendTimeoutException(), completionSource);
                return false;
            }
        }
        protected bool TrySendMultipartBytes(params byte[][] frames) {
            if (Socket.TrySendMultipartBytes(SendReceiveTimeout, frames))
                return true;
            else {
                HandleError(new SendTimeoutException());
                return false;
            }
        }
        protected bool TrySendMultipartBytes<T>(byte[][] frames, TaskCompletionSource<T> completionSource) {
            if (Socket.TrySendMultipartBytes(SendReceiveTimeout, frames))
                return true;
            else {
                HandleError(new SendTimeoutException(), completionSource);
                return false;
            }
        }
        protected bool TrySendFrameBytes(byte[] frame) {
            if (Socket.TrySendFrame(SendReceiveTimeout, frame))
                return true;
            else {
                HandleError(new SendTimeoutException());
                return false;
            }
        }
        protected bool TrySendFrameBytes<T>(byte[] frame, TaskCompletionSource<T> completionSource) {
            if (Socket.TrySendFrame(SendReceiveTimeout, frame))
                return true;
            else {
                HandleError(new SendTimeoutException(), completionSource);
                return false;
            }
        }

        protected bool TrySendInvocation(IInvocation invocation) =>
            TrySendMultipartBytes(ServiceUtils.SerializeInvocation(invocation));
        protected bool TrySendInvocation<T>(IInvocation invocation, TaskCompletionSource<T> completionSource) =>
            TrySendMultipartBytes(ServiceUtils.SerializeInvocation(invocation), completionSource);
        protected bool TryReceiveInvocation(out byte[] service, out byte[] method, out object[] arguments) {
            if (TryReceiveMultipartBytes(out List<byte[]> frames, 3)) {
                ServiceUtils.DeserializeInvocation(frames, out service, out method, out arguments);
                return true;
            }
            service = null;
            method = null;
            arguments = null;
            return false;
        }
        protected bool TryReceiveInvocation<T>(IInvocation invocation, TaskCompletionSource<T> completionSource) =>
            TrySendMultipartBytes(ServiceUtils.SerializeInvocation(invocation), completionSource);

        protected void HandleError(Exception exception) {
            switch (ErrorAction) {
                case ErrorAction.Silent:
                    break;
                case ErrorAction.Exception:
                    throw exception ?? new Exception(@"An unknown error occured.");
            }
        }
        protected void HandleError<T>(Exception exception, TaskCompletionSource<T> completionSource) =>
            completionSource?.TrySetException(exception ?? new Exception(@"An unknown error occured."));
        
        #endregion

        #region IDisposable Support
        private bool _IsDisposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!_IsDisposed) {
                if (disposing) {
                    Poller?.Dispose();
                    Socket?.Dispose();
                }
                _IsDisposed = true;
            }
        }
        public void Dispose() => Dispose(true);
        #endregion

    }
}
