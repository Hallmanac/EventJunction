//////////////////////////////////////////
// This code originally done by Matthias Jauernig as part of a blog post about an implmentation of the Event Aggregator pattern.
// Slight modifications have been done by Brian Hall.
// Original blog post entry:
// http://www.minddriven.de/index.php/technology/development/design-patterns/event-aggregator-implementation
//////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EvtJunction.Aggregator
{
	public class EventAggregator : IEventAggregator
	{
		private readonly object _lock = new object();
		private readonly IDictionary<Type, IList> _subscriptions = new Dictionary<Type, IList>();
		private static readonly IEventAggregator CurrentInstance = new EventAggregator();

		public static IEventAggregator Current { get { return CurrentInstance; } }

		public void Publish<TMessage>(TMessage message) where TMessage : IApplicationEvent
		{
			if(message == null)
				throw new ArgumentNullException("message");
			lock(_lock)
			{
				var messageType = typeof(TMessage);
				if(_subscriptions.ContainsKey(messageType))
				{
					var subscriptionList = new List<ISubscription<TMessage>>(
						_subscriptions[messageType].Cast<ISubscription<TMessage>>());
					foreach(var subscription in subscriptionList)
					{
						subscription.Action(message);
					}
				}
			}
		}

		public ISubscription<TMessage> Subscribe<TMessage>(Action<TMessage> action)
			where TMessage : IApplicationEvent
		{
			lock(_lock)
			{
				var messageType = typeof(TMessage);
				var subscription = new Subscription<TMessage>(this, action);
				if(_subscriptions.ContainsKey(messageType))
					_subscriptions[messageType].Add(subscription);
				else
					_subscriptions.Add(messageType, new List<ISubscription<TMessage>> {subscription});
				return subscription;
			}
		}

		public void UnSubscribe<TMessage>(ISubscription<TMessage> subscription)
			where TMessage : IApplicationEvent
		{
			var messageType = typeof(TMessage);
			lock(_lock)
			{
				if(_subscriptions.ContainsKey(messageType))
					_subscriptions[messageType].Remove(subscription);
			}
		}

		public void ClearAllSubscriptions() { ClearAllSubscriptions(null); }

		public void ClearAllSubscriptions(Type[] exceptMessages)
		{
			foreach(var messageSubscriptions in new Dictionary<Type, IList>(_subscriptions))
			{
				var canDelete = true;
				if(exceptMessages != null)
					canDelete = !exceptMessages.Contains(messageSubscriptions.Key);
				if(!canDelete)
					continue;
				lock(_lock) 
				{
					_subscriptions.Remove(messageSubscriptions);
				}
			}
		}
	}
#region -- First attempt (Commented out) --

	/*public class EventAggregator : IEventAggregator
	{
		private readonly ConcurrentDictionary<Type, List<object>> _subscriptions = new ConcurrentDictionary<Type, List<object>>();

		public static IEventAggregator Current { get { return AppServiceLocator.Current.GetInstance<IEventAggregator>(); } }

		public void Publish<T>(T message) where T : IApplicationEvent
		{
			var thisLock = new object();
			var subscribers = GetSubscribers(typeof(T));

			// To Array creates a copy in case someone unsubscribes in their own handler
			foreach(var objectSubscriber in subscribers.ToArray())
			{
				var subscriber = (Action<T>)objectSubscriber;
				var syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
				syncContext.Post(state => subscriber(message), null);
			}
		}

		public void Subscribe<T>(Action<T> callback) where T : IApplicationEvent
		{
			var thisLock = new object();
			lock(thisLock)
			{
				var subscribers = GetSubscribers(typeof(T));
				subscribers.Add(callback);
			}
		}

		public void Unsubscribe<T>(Action<T> callback) where T : IApplicationEvent
		{
			var subscribers = GetSubscribers(typeof(T));
			var thisLock = new object();
			lock(thisLock)
			{
				object existing = null;
				foreach(var s in subscribers.Cast<Action<T>>().Where(s => s.Method.Name == callback.Method.Name))
				{
					existing = s;
				}
				subscribers.Remove(existing);
			}
		}

		private List<object> GetSubscribers(Type subscriberType)
		{
			List<object> subscribers;
			var thisLock = new object();
			lock(thisLock)
			{
				var found = _subscriptions.TryGetValue(subscriberType, out subscribers);
				if(found)
					return subscribers;
				subscribers = new List<object>();
				_subscriptions.AddOrUpdate(subscriberType, subscribers, (type, oldValue) => subscribers);
			}
			return subscribers;
		}
	}*/
#endregion
}