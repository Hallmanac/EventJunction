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

        //public static IEventAggregator Current { get { return AppServiceLocator.Current.GetInstance<IEventAggregator>(); } }
        public static IEventAggregator Current { get; } = new EventAggregator();


        public void Publish<TMessage>(TMessage message) where TMessage : IApplicationEvent
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            lock (_lock)
            {
                var messageType = typeof(TMessage);
                if (_subscriptions.ContainsKey(messageType))
                {
                    var subscriptionList = new List<ISubscription<TMessage>>(
                        _subscriptions[messageType].Cast<ISubscription<TMessage>>());
                    foreach (var subscription in subscriptionList)
                    {
                        subscription.Action(message);
                    }
                }
            }
        }


        public ISubscription<T> Subscribe<T>(Action<T> action, Guid correlationId = default(Guid)) where T : IApplicationEvent
        {
            lock (_lock)
            {
                var messageType = typeof(T);
                var subscription = new Subscription<T>(this, action, correlationId);
                if (_subscriptions.ContainsKey(messageType))
                {
                    // Check to see if there is already a subscription of the same type with the same correlation ID
                    if (correlationId != default(Guid))
                    {
                        var hasCorrelationID = false;
                        var subsList = _subscriptions[messageType];
                        foreach (Subscription<T> subscriptionItem in subsList)
                        {
                            hasCorrelationID = subscriptionItem.CorrelationId == correlationId && subscriptionItem.Action == action;
                            if (hasCorrelationID)
                            {
                                subscription = subscriptionItem;
                            }
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
                    _subscriptions.Add(messageType, new List<ISubscription<T>> { subscription });
                }
                return subscription;
            }
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
                    _subscriptions.Remove(messageSubscriptions);
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
                _subscriptions.Remove(evtSubscriptions);
            }
        }
    }
}