using Compliance360.EmployeeSync.Library;
using NUnit.Framework;
using StructureMap;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class ContainerTest
    {
        [TearDown]
        public void Cleanup()
        {
            ContainerFactory.Reset();
        }

        [Test]
        public void TestContainerFactory()
        {
            var container = ContainerFactory.GetContainer();
            Assert.IsNotNull(container);
        }

        [Test]
        public void TestContainerFactoryWithRegistry()
        {
            var registry = new Registry();
            registry.For<ITestClass>().Use<TestClass>();

            var container = ContainerFactory.GetContainer(registry);
            var myClass = container.GetInstance<ITestClass>();
            Assert.True(myClass is TestClass);
        }

        [Test]
        public void TestContainerFactoryWithRegistryWithReset()
        {
            // get the default contianer 
            ContainerFactory.GetContainer();

            // use custom registry
            var registry = new Registry();
            registry.For<ITestClass>().Use<TestClass>();

            // ensure we get class in the new registry
            var container = ContainerFactory.GetContainer(registry);
            var myClass = container.GetInstance<ITestClass>();
            Assert.True(myClass is TestClass);
        }
    }

    public interface ITestClass
    {
    }

    public class TestClass : ITestClass
    {
    }
}