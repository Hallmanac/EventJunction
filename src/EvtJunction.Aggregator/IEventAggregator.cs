//////////////////////////////////////////
// This code originally done by Matthias Jauernig as part of a blog post about an implmentation of the Event Aggregator pattern.
// Slight modifications have been done by Brian Hall.
// Original blog post entry:
// http://www.minddriven.de/index.php/technology/development/design-patterns/event-aggregator-implementation
//////////////////////////////////////////

using System;

namespace EvtJunction.Aggregator
{
	public interface IEventAggregator
	{
		void Publish<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent;

		ISubscription<TAppEvent> Subscribe<TAppEvent>(Action<TAppEvent> callback) where TAppEvent : IApplicationEvent;

		void UnSubscribe<TAppEvent>(ISubscription<TAppEvent> subscription) where TAppEvent : IApplicationEvent;

		void ClearAllSubscriptions();

		void ClearAllSubscriptions(Type[] exceptMessages); 
	}
}