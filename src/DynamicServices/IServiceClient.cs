using System;

namespace OpenByte.DynamicServices {
    public interface IServiceClient {

        object GetServiceProxy(Type type, Type[] additionalInterfaces);

    }
}
