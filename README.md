# DynamicServices

[![NetMQ NuGet version](https://img.shields.io/nuget/v/DynamicServices.svg)](https://www.nuget.org/packages/DynamicServices/)

## Installation

You can download DynamicServices via [NuGet](https://nuget.org/packages/DynamicServices/).


#### Client - Host

```csharp
var address = @"localhost";
var port = 5000;
using (var host = new ServiceHost(address, port)) {
    host.RegisterService<EchoService>();
    host.Start();
    using (var client = new ServiceClient(address, port)) {
        var service = client.GetServiceProxy<IEchoService>();
        client.Start();

        Console.WriteLine(service.Echo("Hello World!"));

        client.Shutdown();
    }
    host.Shutdown();
}

// Service Definition
public interface IEchoService {

    string Echo(string text);

}

// Service Implementation
public class EchoService : IEchoService {

    public string Echo(string text) => text;

}
```



#### Publish - Subscribe

```csharp
var address = @"localhost";
var port = 5000;
using (var host = new PublisherService(address, port)) {
    var proxy = host.GetServiceProxy<ILogService>();
    host.Start();
    using (var client = new SubscriptionServiceHost(address, port)) {
        client.RegisterService(service);
        client.Start();

        service.Log("Hello World!");

        client.Shutdown();
    }
    host.Shutdown();
}

// Service Definition
public interface ILogService {

    void Log(string text);

}

// Service Implementation
public class LogService : ILogService {

    public void Log(string text) => Console.WriteLine(text);

}
```
