using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Threax.AspNetCore.Tests
{
    /// <summary>
    /// This class will create mocks using the passed in types. Any type requested from this class will return
    /// a mock. Getting a type from this class multiple times will return the same instance. Finally you can customize
    /// what is returned when a type is requested with the add function. These customizations will use the .net core dependency
    /// injection library.
    /// </summary>
    public class Mockup : IDisposable
    {
        private Dictionary<Type, Object> createdObjects = new Dictionary<Type, Object>(); //Objects that were created directly
        private ServiceCollection mockServiceCollection = new ServiceCollection();
        private ServiceProvider serviceProvider = null;
        private IServiceScope scope = null;

        /// <summary>
        /// Dispose all created objects.
        /// </summary>
        public void Dispose()
        {
            scope?.Dispose();
            serviceProvider?.Dispose();
            foreach (var o in createdObjects.Values.Select(i => i as IDisposable).Where(i => i != null))
            {
                o.Dispose();
            }
        }

        /// <summary>
        /// Add a custom function that is called when a type is requested and creates it. This can return anything
        /// as long as its an instance of T (could be a mock or real object). This function will always replace
        /// any previously registered callback. Anything registered this way will be added as a singleton.
        /// You can call add as much as you want until you start calling Get any services added after the first call to Get
        /// will be ignored.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="cb">The callback to call when type needs to be created.</param>
        public void Add<T>(Func<Mockup, T> cb)
        {
            if(serviceProvider != null)
            {
                throw new InvalidOperationException($"Cannot register a type after calling '{nameof(Get)}' once.");
            }

            mockServiceCollection.AddSingleton(typeof(T), s => cb(this));
        }

        /// <summary>
        /// Add a custom function that is called when a type is requested and creates it. This can return anything
        /// as long as its an instance of T (could be a mock or real object). This function will always replace
        /// any previously registered callback. Anything registered this way will be added as a singleton.
        /// You can call add as much as you want until you start calling Get any services added after the first call to Get
        /// will be ignored.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="cb">The callback to call when type needs to be created.</param>
        public void TryAdd<T>(Func<Mockup, T> cb)
        {
            if (serviceProvider != null)
            {
                throw new InvalidOperationException($"Cannot register a type after calling '{nameof(Get)}' once.");
            }

            mockServiceCollection.TryAddSingleton(typeof(T), s => cb(this));
        }

        /// <summary>
        /// Create or retrieve an instance of T. Once you start getting services you should not add any more since they will be ignored.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <returns>The instance of T.</returns>
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// Create an object based on a runtime type. This will return what you previously registered or an empty
        /// mock will be created and returned. This prevents you from having to create mocks for every possible
        /// object yourself. Once you start getting services you should not add any more since they will be ignored.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>A new instance of t.</returns>
        public Object Get(Type t)
        {
            Object service;
            if (!createdObjects.TryGetValue(t, out service))
            {
                if (serviceProvider == null)
                {
                    serviceProvider = mockServiceCollection.BuildServiceProvider();
                    scope = serviceProvider.CreateScope();
                }
                service = scope.ServiceProvider.GetService(t);
                if (service == null)
                {
                    //Auto create a mock for any types not registered
                    service = (Activator.CreateInstance(typeof(Mock<>).MakeGenericType(new Type[] { t })) as Mock).Object;
                    createdObjects.Add(t, service); //Only track mocks created directly
                }
            }
            return service;
        }

        /// <summary>
        /// Get the underlying service collection to use directly if testing a library that configures services itself. Note
        /// that the service collection returned is not aware of the auto mocking capabilities of this class so if your library
        /// needs to have additional mocks you will have to register them yourself.
        /// </summary>
        public IServiceCollection MockServiceCollection { get => mockServiceCollection; }
    }
}
