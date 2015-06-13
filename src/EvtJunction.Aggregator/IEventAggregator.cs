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