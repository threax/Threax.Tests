using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Tests.Tests
{
    public interface ITestInterface
    {
        bool DoSomething();
    }

    class TestImplementation : ITestInterface
    {
        public bool DoSomething()
        {
            return true;
        }
    }
}
