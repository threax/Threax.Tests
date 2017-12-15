using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Threax.AspNetCore.Tests
{
    /// <summary>
    /// This class will create mocks using the passed in types. Any type requested from this class will return
    /// a mock. Getting a type from this class multiple times will return the same instance. Finally you can customize
    /// what is returned when a type is requested with the add function.
    /// </summary>
    public class Mockup : IDisposable
    {
        private Dictionary<Type, Func<Mockup, Object>> customCreateFuncs = new Dictionary<Type, Func<Mockup, object>>();
        private Dictionary<Type, Object> createdObjects = new Dictionary<Type, Object>();

        /// <summary>
        /// Dispose all created objects.
        /// </summary>
        public void Dispose()
        {
            foreach (var o in createdObjects.Values.Select(i => i as IDisposable).Where(i => i != null))
            {
                o.Dispose();
            }
        }

        /// <summary>
        /// Add a custom function that is called when a type is requested and creates it. This can return anything
        /// as long as its an instance of T (could be a mock or real object). This function will always replace
        /// any previously registered callback.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="cb">The callback to call when type needs to be created.</param>
        public void Add<T>(Func<Mockup, T> cb)
        {
            customCreateFuncs[typeof(T)] = m => cb(m);
        }

        /// <summary>
        /// Create or retrieve an instance of T.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <returns>The instance of T.</returns>
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// Create an object based on a runtime type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>A new instance of t.</returns>
        public Object Get(Type t)
        {
            Object created;
            if (!createdObjects.TryGetValue(t, out created))
            {
                Func<Mockup, Object> createFunc;
                if (customCreateFuncs.TryGetValue(t, out createFunc))
                {
                    created = createFunc(this);
                }
                else
                {
                    created = (Activator.CreateInstance(typeof(Mock<>).MakeGenericType(new Type[] { t })) as Mock).Object;
                }
                createdObjects.Add(t, created);
            }
            return created;
        }
    }
}
