//////////////////////////////////////////
// This code originally done by Matthias Jauernig as part of a blog post about an implmentation of the Event Aggregator pattern.
// Slight modifications have been done by Brian Hall.
// Original blog post entry:
// http://www.minddriven.de/index.php/technology/development/design-patterns/event-aggregator-implementation
//////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nito.AsyncEx.Synchronous;


namespace EvtJunction.Aggregator
{
	public class EventAggregator : IEventAggregator
	{
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Type, IList> _subscriptions = new ConcurrentDictionary<Type, IList>();

        //public static IEventAggregator Current { get { return AppServiceLocator.Current.GetInstance<IEventAggregator>(); } }
        public static IEventAggregator Current { get; } = new EventAggregator();


        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : IApplicationEvent
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = typeof(TMessage);
            List<ISubscription<TMessage>> subscriptionList = null;
            lock (_lock)
            {
                if (_subscriptions.ContainsKey(messageType))
                {
                    subscriptionList = new List<ISubscription<TMessage>>(
                        _subscriptions[messageType].Cast<ISubscription<TMessage>>());
                }
            }
            if (subscriptionList != null)
            {
                foreach (var subscription in subscriptionList)
                {
                    await subscription.Action(message);
                }
            }
        }


        public void Publish<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent
        {
            PublishAsync(message).WaitAndUnwrapException();
        }


        public ISubscription<T> Subscribe<T>(Func<T, Task> action, Guid correlationId = default(Guid)) where T : IApplicationEvent
        {
            lock (_lock)
            {
                var messageType = typeof(T);
                var subscription = new Subscription<T>(this, action, correlationId);

                // Check to see if we have this event message type defined in the subscriptions dictionary. If not, create the
                // event message type and add the subscription to it.
                if (_subscriptions.ContainsKey(messageType))
                {
                    // Check to see if there is already a subscription of the same type with the same correlation ID
                    if (correlationId != default(Guid))
                    {
                        var hasCorrelationID = false;
                        var subsList = _subscriptions[messageType];

                        // Iterate through the subscriptions and determine if there is an existing subscription with that correlation ID
                        foreach (Subscription<T> subscriptionItem in subsList)
                        {
                            // We check for a match on both the correlation Id and the action (or method to be called when event is published)
                            hasCorrelationID = subscriptionItem.CorrelationId == correlationId && subscriptionItem.Action == action;

                            // If we don't have a correlation Id then we continue with the loop
                            if (!hasCorrelationID)
                            {
                                continue;
                            }

                            // Otherwise we we pull the existing subscription out of the current list and return 
                            // that back to the subscriber.
                            subscription = subscriptionItem;
                        }
                        if (!hasCorrelationID)
                        {
                            _subscriptions[messageType].Add(subscription);
                        }
                    }
                    else
                    {
                        _subscriptions[messageType].Add(subscription);
                    }
                }
                else
                {
                    _subscriptions.GetOrAdd(messageType, new List<ISubscription<T>> { subscription });
                }
                return subscription;
            }
        }


        public ISubscription<TAppEvent> SubscribeSynchronousMethod<TAppEvent>(Action<TAppEvent> callback, Guid correlationId = new Guid())
            where TAppEvent : IApplicationEvent
        {
            return Subscribe<TAppEvent>(evtMsg => Task.Run(() => callback(evtMsg)), correlationId);
        }


        public void UnSubscribe<T>(ISubscription<T> subscription) where T : IApplicationEvent
        {
            var messageType = typeof(T);
            lock (_lock)
            {
                if (_subscriptions.ContainsKey(messageType))
                {
                    _subscriptions[messageType].Remove(subscription);
                }
            }
        }


        public void ClearAllSubscriptions() { ClearAllSubscriptions(null); }


        public void ClearAllSubscriptions(Type[] exceptMessages)
        {
            foreach (var messageSubscriptions in new Dictionary<Type, IList>(_subscriptions))
            {
                var canDelete = true;
                if (exceptMessages != null)
                {
                    canDelete = !exceptMessages.Contains(messageSubscriptions.Key);
                }
                if (!canDelete)
                {
                    continue;
                }
                lock (_lock)
                {
                    IList outList;
                    _subscriptions.TryRemove(messageSubscriptions.Key, out outList);
                }
            }
        }


        public void ClearAllCorrelatedSubscriptions(Guid correlationId)
        {
            var subs = new Dictionary<Type, IList>(_subscriptions);
            foreach (var subscriberList in subs)
            {
                var subList = Activator.CreateInstance(subscriberList.Value.GetType()) as IList;
                if (subList == null)
                {
                    continue;
                }
                foreach (var item in subscriberList.Value)
                {
                    subList.Add(item);
                }

                foreach (var subscription in subList)
                {
                    var subType = subscription.GetType();
                    if (subType.GetInterface(typeof(ISubscription<>).Name) == null)
                    {
                        continue;
                    }
                    var correlationIdProp = subType.GetProperties().ToList().FirstOrDefault(info => info.Name == "CorrelationId");
                    if (correlationIdProp == null)
                    {
                        continue;
                    }
                    var idVal = correlationIdProp.GetValue(subscription, null) is Guid
                        ? (Guid)correlationIdProp.GetValue(subscription, null)
                        : new Guid();
                    if (idVal != correlationId)
                    {
                        continue;
                    }
                    lock (_lock)
                    {
                        if (_subscriptions.ContainsKey(subscriberList.Key))
                        {
                            _subscriptions[subscriberList.Key].Remove(subscription);
                        }
                    }
                }
            }
        }


        public void ClearSubscriptionsFor<T>()
        {
            var subs = new Dictionary<Type, IList>(_subscriptions);
            if (!subs.ContainsKey(typeof(T)))
            {
                return;
            }
            var evtSubscriptions = subs.FirstOrDefault(messageSubscriptions => messageSubscriptions.Key == typeof(T));
            lock (_lock)
            {
                IList outList;
                _subscriptions.TryRemove(evtSubscriptions.Key, out outList);
            }
        }
    }
}