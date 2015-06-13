using System;

namespace EvtJunction.Aggregator
{
	public class Subscription<TMessage> : ISubscription<TMessage> where TMessage : IApplicationEvent
	{
		private bool _disposed;
		private Guid _eventId;

		public Subscription(IEventAggregator eventAggregator, Action<TMessage> action)
		{
			if (eventAggregator == null)
				throw new ArgumentNullException("eventAggregator");
			if (action == null)
				throw new ArgumentNullException("action");
			EventAggregator = eventAggregator;
			Action = action;
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

		public Action<TMessage> Action { get; private set; }

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