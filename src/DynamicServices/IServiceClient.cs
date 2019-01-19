using System;

namespace DynamicServices {
    public interface IServiceClient {

        object GetServiceProxy(Type type, Type[] additionalInterfaces);

    }
}
