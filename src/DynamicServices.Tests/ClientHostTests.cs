using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicServices.Tests {
    [TestClass]
    public class ClientHostTests {

        public ClientHostTests() => DynamicServicesConfig.Cleanup();

        [TestMethod]
        public void SimpleClientHost() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                var port = host.BindRandomPort(address);
                host.RegisterService<EchoService>();
                host.Start();
                using (var client = new ServiceClient()) {
                    client.Connect(address, port);
                    var service = client.GetServiceProxy<IEchoService>();
                    client.Start();

                    EchoTest("fejkfehsjkfeshjkhjkfhesjkfhjkh");
                    EchoTest("̀(╯°□°）╯︵ ┻━┻");

                    void EchoTest(string value) {
                        var echo = service.Echo(value);
                        Assert.AreEqual(value, echo);
                    }

                    client.Shutdown();
                }
                host.Shutdown();
            }
        }

        public class EchoService : IEchoService {

            public string Echo(string text) => text;

        }
        public interface IEchoService {

            string Echo(string text);

        }

    }
}
