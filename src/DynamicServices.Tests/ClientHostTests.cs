using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace OpenByte.DynamicServices.Tests {
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

                    EchoTest(42);
                    EchoTest(42424242424242L);
                    EchoTest(42.42f);
                    EchoTest(new byte[] { 1, 2, 3, 4, 5 });
                    EchoTest("fejkfehsjkfeshjkhjkfhesjkfhjkh");
                    EchoTest("̀(╯°□°）╯︵ ┻━┻");
                    EchoTest(("fhefshj", 42));
                    void EchoTest(object value) {
                        var echo = service.Echo(value);
                        Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(value, echo));
                    }

                    client.Shutdown();
                }
                host.Shutdown();
            }
        }

        [TestMethod]
        public void AsyncClientHost() {
            _AsyncClientHost().Wait();
        }
        public async Task _AsyncClientHost() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                var port = host.BindRandomPort(address);
                host.RegisterService<EchoService>();
                host.Start();
                using (var client = new ServiceClient()) {
                    client.Connect(address, port);
                    var service = client.GetServiceProxy<IEchoService>();
                    client.Start();

                    await EchoTest(42);
                    await EchoTest(new byte[] { 1, 2, 3, 4, 5 });
                    await EchoTest("̀(╯°□°）╯︵ ┻━┻");

                    async Task EchoTest(object value) {
                        var echo = await service.EchoAsync(value);
                        Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(value, echo));
                        var echo2 = await service.EchoAsync2(value);
                        Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(value, echo2));
                    }

                    client.Shutdown();
                }
                host.Shutdown();
            }
        }

        public class EchoService : IEchoService {

            public object Echo(object obj) => obj;

            public async Task<object> EchoAsync(object obj) {
                await Task.Delay(10);
                return obj;
            }
            public ValueTask<object> EchoAsync2(object obj) => new ValueTask<object>(obj);

        }
        public interface IEchoService {

            object Echo(object obj);
            Task<object> EchoAsync(object obj);
            ValueTask<object> EchoAsync2(object obj);

        }


    }
}
