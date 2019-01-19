using System;

namespace DynamicServices {
    public interface IServiceHost {

        void RegisterService(object service, Type[] interfaces);
        bool UnregisterService(Type service);

    }
}
