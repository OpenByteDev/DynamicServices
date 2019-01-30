using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OpenByte.DynamicServices.Tests {
    [TestClass]
    public class PubSubTests {

        public PubSubTests() => DynamicServicesConfig.Cleanup();

        [TestMethod]
        public void SimplePubSub() {
            var address = @"localhost";
            using (var host = new PublisherService()) {
                var port = host.BindRandomPort(address);
                var proxy = host.GetServiceProxy<ILogService>();
                var service = new LogService();
                host.Start();
                using (var client = new SubscriptionServiceHost()) {
                    client.Connect(address, port);
                    client.RegisterService(service);
                    client.Start();

                    service.Log("fejkfehsjkfeshjkhjkfhesjkfhjkh");
                    service.Log("̀(╯°□°）╯︵ ┻━┻");

                    Assert.AreEqual(service.Logs.Count, 2);

                    client.Shutdown();
                }
                host.Shutdown();
            }

        }

        public class LogService : ILogService {

            public List<string> Logs = new List<string>();

            public void Log(string text) => Logs.Add(text);

        }
        public interface ILogService {

            void Log(string text);

        }

    }
}
