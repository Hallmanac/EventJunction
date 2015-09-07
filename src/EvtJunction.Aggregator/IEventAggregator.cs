//////////////////////////////////////////
// This code originally done by Matthias Jauernig as part of a blog post about an implmentation of the Event Aggregator pattern.
// Slight modifications have been done by Brian Hall.
// Original blog post entry:
// http://www.minddriven.de/index.php/technology/development/design-patterns/event-aggregator-implementation
//////////////////////////////////////////

using System;
using System.Threading.Tasks;


namespace EvtJunction.Aggregator
{
	public interface IEventAggregator
	{
        Task PublishAsync<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent;


        void Publish<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent;


        ISubscription<TAppEvent> Subscribe<TAppEvent>(Func<TAppEvent, Task> callback, Guid correlationId = default(Guid))
            where TAppEvent : IApplicationEvent;

        ISubscription<TAppEvent> SubscribeSynchronousMethod<TAppEvent>(Action<TAppEvent> callback, Guid correlationId = default(Guid))
            where TAppEvent : IApplicationEvent;


        void UnSubscribe<TAppEvent>(ISubscription<TAppEvent> subscription) where TAppEvent : IApplicationEvent;


        void ClearAllSubscriptions();


        void ClearAllSubscriptions(Type[] exceptMessages);


        void ClearSubscriptionsFor<T>();


        void ClearAllCorrelatedSubscriptions(Guid correlationId);
    }
}