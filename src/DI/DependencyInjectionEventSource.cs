using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection
{
    [EventSource(Name = "Microsoft-Extensions-DependencyInjection")]
    internal class DependencyInjectionEventSource : EventSource
    {
        public static readonly DependencyInjectionEventSource Log = new DependencyInjectionEventSource();

        private readonly EventCounter _serviceRealizationDuration;
        private readonly EventCounter _servicesRealized;
        private readonly EventCounter _servicesResolved;
        private readonly EventCounter _callSiteDepth;

        private DependencyInjectionEventSource()
        {
            _serviceRealizationDuration = new EventCounter("ServiceRealizationDuration", this);
            _servicesRealized = new EventCounter("ServicesRealized", this);
            _servicesResolved = new EventCounter("ServicesResolved", this);
            _callSiteDepth = new EventCounter("CallSiteDepth", this);
        }

        [NonEvent]
        public void RealizingService(IServiceCallSite callSite)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                RealizingService(
                    callSite.ServiceType.AssemblyQualifiedName,
                    callSite.ImplementationType?.AssemblyQualifiedName ?? string.Empty,
                    callSite.GetType().AssemblyQualifiedName);
            }
        }

        [NonEvent]
        public void RealizedService(IServiceCallSite callSite, object instance, TimeSpan duration)
        {
            if (IsEnabled())
            {
                _serviceRealizationDuration.WriteMetric((float)duration.TotalMilliseconds);
                _servicesRealized.WriteMetric(1.0f);

                if (IsEnabled(EventLevel.Informational, EventKeywords.None))
                {
                    var instanceType = instance.GetType();
                    RealizedService(
                        callSite.ServiceType.AssemblyQualifiedName,
                        instanceType.AssemblyQualifiedName,
                        callSite.GetType().AssemblyQualifiedName,
                        duration.TotalMilliseconds);
                }
            }
        }

        [NonEvent]
        internal void DisposedService(IDisposable disposable)
        {
            if(IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                DisposedService(disposable.GetType().AssemblyQualifiedName);
            }
        }

        [NonEvent]
        public void ResolvingService(Type serviceType)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                ResolvingService(serviceType.AssemblyQualifiedName);
            }
        }

        [NonEvent]
        public void ResolvedService(Type serviceType, object instance, TimeSpan duration)
        {
            if (IsEnabled())
            {
                _servicesResolved.WriteMetric(1.0f);

                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    ResolvedService(
                        serviceType.AssemblyQualifiedName,
                        instance?.GetType()?.AssemblyQualifiedName ?? "<null>",
                        duration.TotalMilliseconds);
                }
            }
        }

        [Event(eventId: 1, Level = EventLevel.Informational, Message = "Realizing service '{0}'")]
        private void RealizingService(string serviceType, string expectedImplementationType, string callSiteType) => WriteEvent(1, serviceType, expectedImplementationType, callSiteType);

        [Event(eventId: 2, Level = EventLevel.Informational, Message = "Realized service '{0}' using implementation type '{1}' in {3}ms")]
        private void RealizedService(string serviceType, string implementationType, string callSiteType, double durationInMilliseconds) => WriteEvent(2, serviceType, implementationType, callSiteType, durationInMilliseconds);

        [Event(eventId: 3, Level = EventLevel.Informational, Message = "Resolving service '{0}'")]
        private void ResolvingService(string serviceType) => WriteEvent(3, serviceType);

        [Event(eventId: 4, Level = EventLevel.Informational, Message = "Resolved service '{0}' using implementation type '{1}' in {2}ms")]
        private void ResolvedService(string serviceType, string implementationType, double durationInMilliseconds) => WriteEvent(4, serviceType, implementationType, durationInMilliseconds);

        [Event(eventId: 5, Level = EventLevel.Verbose, Message = "Service Provider Scope started")]
        internal void ScopeStarted()
        {
            if(IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(5);
            }
        }

        [Event(eventId: 6, Level = EventLevel.Verbose, Message = "Service Provider Scope ended")]
        internal void ScopeEnded()
        {
            if(IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(6);
            }
        }

        [Event(eventId: 7, Level = EventLevel.Verbose, Message = "Disposed service '{0}'")]
        private void DisposedService(string serviceType) => WriteEvent(7, serviceType);
    }
}
