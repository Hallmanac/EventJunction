//////////////////////////////////////////
// This code originally done by Matthias Jauernig as part of a blog post about an implmentation of the Event Aggregator pattern.
// Slight modifications have been done by Brian Hall.
// Original blog post entry:
// http://www.minddriven.de/index.php/technology/development/design-patterns/event-aggregator-implementation
//////////////////////////////////////////

using System;

namespace EvtJunction.Aggregator
{
	public interface ISubscription<TMessage> : IDisposable, IEquatable<ISubscription<TMessage>> where TMessage : IApplicationEvent
	{
        Guid EventId { get; }

        Action<TMessage> Action { get; }

        IEventAggregator EventAggregator { get; }

        Guid CorrelationId { get; set; }
    }
}