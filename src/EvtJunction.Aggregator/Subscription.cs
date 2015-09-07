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
	public class Subscription<TMessage> : ISubscription<TMessage> where TMessage : IApplicationEvent
	{
        private bool _disposed;
        private Guid _eventId;

        public Subscription(IEventAggregator eventAggregator, Func<TMessage, Task> action, Guid correlationId = default(Guid))
        {
            if (eventAggregator == null)
                throw new ArgumentNullException(nameof(eventAggregator));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            EventAggregator = eventAggregator;
            Action = action;
            CorrelationId = correlationId == default(Guid) ? Guid.NewGuid() : correlationId;
        }

        public Guid EventId
        {
            get
            {
                if (_eventId == Guid.Empty)
                    _eventId = Guid.NewGuid();
                return _eventId;
            }
        }

        public Guid CorrelationId { get; set; }

        public Func<TMessage, Task> Action { get; private set; }

        public IEventAggregator EventAggregator { get; private set; }

        #region -- Dispose Implementation --
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (!disposing)
                return;
            EventAggregator.UnSubscribe(this);
            _disposed = true;
        }
        #endregion

        #region -- Equality Implementation --
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Subscription<TMessage>)obj);
        }

        public bool Equals(ISubscription<TMessage> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return _eventId.Equals(other.EventId);
        }

        public override int GetHashCode() { return EventId.GetHashCode(); }

        public static bool operator ==(Subscription<TMessage> left, Subscription<TMessage> right) { return Equals(left, right); }

        public static bool operator !=(Subscription<TMessage> left, Subscription<TMessage> right) { return !Equals(left, right); }
        #endregion
    }
}