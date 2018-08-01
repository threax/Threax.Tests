using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.AspNetCore.Tests
{
    class MockServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}
