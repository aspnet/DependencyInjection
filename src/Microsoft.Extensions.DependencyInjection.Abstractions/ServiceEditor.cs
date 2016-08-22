using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceEditor<T> : IServicesEditor<T>
    {
        public Type ServiceType { get; set; }

        private readonly IList<ServiceDescriptor> _collection;

        public ServiceEditor(IList<ServiceDescriptor> collection) : this(collection, typeof(T))
        {
        }

        public ServiceEditor(IList<ServiceDescriptor> collection, Type serviceType)
        {
            ServiceType = serviceType;
            _collection = collection;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            _collection.Add(item);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return _collection.Remove(item);
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public int IndexOf(ServiceDescriptor item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _collection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }

        public ServiceDescriptor this[int index]
        {
            get { return _collection[index]; }
            set { _collection[index] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}