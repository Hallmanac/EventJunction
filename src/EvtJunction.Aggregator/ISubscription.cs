using System;

namespace EvtJunction.Aggregator
{
	public interface ISubscription<TMessage> : IDisposable, IEquatable<ISubscription<TMessage>> where TMessage : IApplicationEvent
	{
		Guid EventId { get; }

		Action<TMessage> Action { get; }

		IEventAggregator EventAggregator { get; } 
	}
}