using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetMQ;
using System;

namespace DynamicServices.Tests {
    [TestClass]
    public class ServiceHostTests {
        
        public ServiceHostTests() => DynamicServicesConfig.Cleanup();

        [TestMethod]
        public void NoDoubleBind() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                var port = host.BindRandomPort(address);
                Assert.ThrowsException<AddressAlreadyInUseException>(() => host.Bind(address, port));
                host.UnbindAll();
            }
        }

        [TestMethod]
        public void NoDoubleStart() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                host.BindRandomPort(address);
                host.Start();
                Assert.ThrowsException<InvalidOperationException>(() => host.Start());
                host.UnbindAll();
            }
        }

        [TestMethod]
        public void NoDoubleStop() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                host.BindRandomPort(address);
                host.Start();
                host.Stop();
                Assert.ThrowsException<InvalidOperationException>(() => host.Stop());
                host.UnbindAll();
            }
        }

        [TestMethod]
        public void TryMethodsDoNotThrow() {
            var address = @"localhost";
            using (var host = new ServiceHost()) {
                host.BindRandomPort(address);
                host.TryStart();
                host.TryStart();
                host.TryStart();
                host.TryShutdown();
                host.TryShutdown();
                host.TryShutdown();
                host.TryStop();
                host.TryStop();
                host.TryStop();
                host.UnbindAll();
            }
        }

        [TestMethod]
        public void NoDoubleRegistration() {
            using (var host = new ServiceHost()) {
                host.RegisterService(new TestService1());
                Assert.ThrowsException<InvalidOperationException>(() => host.RegisterService<TestService2>());
            }
        }

        [TestMethod]
        public void UnregistrationReturnsFound() {
            using (var host = new ServiceHost()) {
                host.RegisterService(new TestService1());
                Assert.IsTrue(host.UnregisterService<TestService1>());
                Assert.IsFalse(host.UnregisterService<TestService1>());
            }
        }

        [TestMethod]
        public void OnlySupportedMethods() {
            using (var host = new ServiceHost()) {
                Assert.ThrowsException<NotSupportedException>(() => host.RegisterService<RichParameterService, IDefaultParameterService>());
                Assert.ThrowsException<NotSupportedException>(() => host.RegisterService<RichParameterService, IRefParameterService>());
                Assert.ThrowsException<NotSupportedException>(() => host.RegisterService<RichParameterService, IOutParameterService>());
                Assert.ThrowsException<NotSupportedException>(() => host.RegisterService<RichParameterService, IInParameterService>());
            }
        }

        public class RichParameterService : IRefParameterService, IOutParameterService, IInParameterService, IDefaultParameterService {
            public void MethodWithDefault(int arg = 0) { }
            public void MethodWithRef(ref int arg) { }

            public void MethodWithOut(out int arg) { arg = 0; }

            public void MethodWithIn(in int arg) { }
        }
        public interface IDefaultParameterService {
            void MethodWithDefault(int arg = 0);
        }
        public interface IRefParameterService {
            void MethodWithRef(ref int arg);
        }
        public interface IOutParameterService {
            void MethodWithOut(out int arg);
        }
        public interface IInParameterService {
            void MethodWithIn(in int arg);
        }

        public class TestService1 : ITestService1 {
            public void Method1() { }
        }
        public class TestService2 : ITestService2 {
            public void Method1() { }
            public void Method2() { }
        }
        public class TestService3 : ITestService3 {
            public void Method1() { }
            public void Method2() { }
            public void Method3() { }
        }
        public interface ITestService1 {
            void Method1();
        }
        public interface ITestService2 : ITestService1 {
            void Method2();
        }
        public interface ITestService3 : ITestService2 {
            void Method3();
        }

    }
}
