using System;
using Ninject.Activation;
using Ninject.Infrastructure.Disposal;
using Ninject.Parameters;
using Ninject.Planning.Targets;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Ninject
{
    internal class ScopeParameter : IParameter, IDisposable, IDisposableObject, INotifyWhenDisposed
    {
        public string Name
        {
            get { return typeof(ScopeParameter).FullName; }
        }

        public bool ShouldInherit
        {
            get { return true; }
        }

        public object GetValue(IContext context, ITarget target)
        {
            return null;
        }

        public bool Equals(IParameter other)
        {
            return this == other;
        }

        public void Dispose()
        {
            var disposed = Disposed;
            if (disposed != null)
            {
                disposed(this, EventArgs.Empty);
            }

            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        public event EventHandler Disposed;
    }
}