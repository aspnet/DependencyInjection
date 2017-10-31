using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    [EventSource(Name = "Microsoft-Extensions-DependencyInjection")]
    internal class DependencyInjectionEventSource : EventSource
    {
        public static readonly DependencyInjectionEventSource Log = new DependencyInjectionEventSource();

        private readonly EventCounter _serviceRealizationDuration;
        private readonly EventCounter _servicesRealized;

        private DependencyInjectionEventSource()
        {
            _serviceRealizationDuration = new EventCounter("ServiceRealizationDuration", this);
            _servicesRealized = new EventCounter("ServicesRealized", this);
        }

        [NonEvent]
        public ValueStopwatch RealizingService(IServiceCallSite callSite)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                RealizingService(
                    callSite.ServiceType.AssemblyQualifiedName,
                    callSite.ImplementationType?.AssemblyQualifiedName ?? string.Empty,
                    callSite.GetType().AssemblyQualifiedName);
                return ValueStopwatch.StartNew();
            }
            return default;
        }

        [NonEvent]
        public void RealizedService(IServiceCallSite callSite, object instance, ValueStopwatch timer)
        {
            if (IsEnabled())
            {
                // REVIEW, TODO: Replace with the below when it finishes in Universe
                //var duration = timer.IsActive ? timer.GetElapsedTime().TotalMilliseconds : 0.0;
                var duration = timer.GetElapsedTime().TotalMilliseconds;
                _serviceRealizationDuration.WriteMetric((float)duration);
                _servicesRealized.WriteMetric(1.0f);

                if (IsEnabled(EventLevel.Informational, EventKeywords.None))
                {
                    var instanceType = instance.GetType();
                    RealizedService(
                        callSite.ServiceType.AssemblyQualifiedName,
                        instanceType.AssemblyQualifiedName,
                        callSite.GetType().AssemblyQualifiedName,
                        duration);
                }
            }
        }

        [NonEvent]
        internal void DisposedService(IDisposable disposable)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                DisposedService(disposable.GetType().AssemblyQualifiedName);
            }
        }

        [Event(eventId: 1, Level = EventLevel.Informational, Message = "Realizing service '{0}'")]
        private void RealizingService(string serviceType, string expectedImplementationType, string callSiteType) => WriteEvent(1, serviceType, expectedImplementationType, callSiteType);

        [Event(eventId: 2, Level = EventLevel.Informational, Message = "Realized service '{0}' using implementation type '{1}' in {3}ms")]
        private void RealizedService(string serviceType, string implementationType, string callSiteType, double durationInMilliseconds) => WriteEvent(2, serviceType, implementationType, callSiteType, durationInMilliseconds);

        [Event(eventId: 3, Level = EventLevel.Verbose, Message = "Service Provider Scope started")]
        internal void ScopeStarted()
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(4);
            }
        }

        [Event(eventId: 5, Level = EventLevel.Verbose, Message = "Service Provider Scope ended")]
        internal void ScopeEnded()
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(5);
            }
        }

        [Event(eventId: 6, Level = EventLevel.Verbose, Message = "Disposed service '{0}'")]
        private void DisposedService(string implementationType) => WriteEvent(6, implementationType);
    }
}
