using NetMQ;
using System;

namespace DynamicServices {
    public static class DynamicServicesConfig {
        
        public static void Cleanup(bool block=true) => NetMQConfig.Cleanup(block);

        public static TimeSpan Linger {
            get => NetMQConfig.Linger;
            set => NetMQConfig.Linger = value;
        }

        public static int ThreadPoolSize {
            get => NetMQConfig.ThreadPoolSize;
            set => NetMQConfig.ThreadPoolSize = value;
        }

        public static int MaxSockets {
            get => NetMQConfig.MaxSockets;
            set => NetMQConfig.MaxSockets = value;
        }

        public static TimeSpan DefaultSendReceiveTimeout = TimeSpan.FromSeconds(10);
        public static TimeSpan DefaultInvocationTimeout = TimeSpan.FromSeconds(20);
        public static ErrorAction DefaultErrorAction = ErrorAction.Exception;

    }
}
