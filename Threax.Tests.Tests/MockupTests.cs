using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using Threax.AspNetCore.Tests;
using Xunit;

namespace Threax.Tests.Tests
{
    public class MockupTests
    {
        [Fact]
        public void ManualMock()
        {
            var mockup = new Mockup();
            mockup.Add<ITestInterface>(s =>
            {
                var mock = new Mock<ITestInterface>();
                mock.Setup(i => i.DoSomething())
                .Returns(true);
                return mock.Object;
            });
            var testInterface = mockup.Get<ITestInterface>();
            Assert.True(testInterface.DoSomething());
        }

        [Fact]
        public void MockOverride()
        {
            //This test just uses exception classes due to easy inheritance
            //not because it actually is exception based

            var mockup = new Mockup();
            mockup.Add<Exception>(s =>
            {
                return new InvalidCastException();
            });

            mockup.Add<Exception>(s =>
            {
                return new InvalidOperationException();
            });

            var test = mockup.Get<Exception>();
            Assert.IsType<InvalidOperationException>(test);
        }

        [Fact]
        public void AutoMock()
        {
            var mockup = new Mockup();
            var testInterface = mockup.Get<ITestInterface>();
            Assert.False(testInterface.DoSomething());
        }

        [Fact]
        public void MockAfterFail()
        {
            //This test just uses exception classes due to easy inheritance
            //not because it actually is exception based

            var mockup = new Mockup();
            mockup.Add<Exception>(s =>
            {
                return new InvalidCastException();
            });

            var test = mockup.Get<Exception>();
            Assert.IsType<InvalidCastException>(test);

            Assert.Throws<InvalidOperationException>(() => {
                mockup.Add<Exception>(s =>
                {
                    return new InvalidOperationException();
                });
            });
        }

        [Fact]
        public void DiTransient()
        {
            var mockup = new Mockup();
            mockup.MockServiceCollection.TryAddTransient<ITestInterface, TestImplementation>();
            var testInterface = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface);
            Assert.True(testInterface.DoSomething());
            var testInterface2 = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface2);
            Assert.True(testInterface2.DoSomething());
            Assert.NotEqual(testInterface, testInterface2); //Make sure transient is working, should get two different instances
        }

        [Fact]
        public void DiScoped()
        {
            var mockup = new Mockup();
            mockup.MockServiceCollection.TryAddScoped<ITestInterface, TestImplementation>();
            var testInterface = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface);
            Assert.True(testInterface.DoSomething());
            var testInterface2 = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface2);
            Assert.True(testInterface2.DoSomething());
            Assert.Equal(testInterface, testInterface2); //Make sure scoped is working, should get the same instance
        }

        [Fact]
        public void DiSingleton()
        {
            var mockup = new Mockup();
            mockup.MockServiceCollection.TryAddSingleton<ITestInterface, TestImplementation>();
            var testInterface = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface);
            Assert.True(testInterface.DoSomething());
            var testInterface2 = mockup.Get<ITestInterface>();
            Assert.NotNull(testInterface2);
            Assert.True(testInterface2.DoSomething());
            Assert.Equal(testInterface, testInterface2); //Make sure scoped is working, should get the same instance
        }
    }
}
