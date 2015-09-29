using System;
using System.Threading.Tasks;

<<<<<<< HEAD
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


=======

namespace EvtJunction.Aggregator
{
    public interface IEventAggregator
    {
        Task PublishAsync<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent;


        /// <summary>
        ///     Publishes an Event Message for the given type <see cref="TAppEvent" />
        /// </summary>
        /// <typeparam name="TAppEvent">
        ///     The Event Message type that is to be published. This must implement an interface for
        ///     <see cref="IApplicationEvent" />
        /// </typeparam>
        /// <param name="message">The event message instance to be published</param>
        void Publish<TAppEvent>(TAppEvent message) where TAppEvent : IApplicationEvent;


        /// <summary>
        ///     This creates a subscription to an event of type <see cref="TAppEvent" /> and returns a reference to the
        ///     subscription that was created. The returned subscription can be used for unsubscribing.
        /// </summary>
        /// <typeparam name="TAppEvent">
        ///     The Event Message type that is to be published. This must implement an interface for
        ///     <see cref="IApplicationEvent" />
        /// </typeparam>
        /// <param name="callback">The method to be called when an event is published for this event type</param>
        /// <param name="correlationId">
        ///     An ID used to track (or correlate) groups of different event types together in order to allow
        ///     for easier management of events (such as unsubscribing all of them at once based on CorrelationID)
        /// </param>
        /// <returns>Reference to subscription that gets created inside the EventAggregator</returns>
        ISubscription<TAppEvent> Subscribe<TAppEvent>(Func<TAppEvent, Task> callback, Guid correlationId = default(Guid))
            where TAppEvent : IApplicationEvent;


        ISubscription<TAppEvent> SubscribeSynchronousMethod<TAppEvent>(Action<TAppEvent> callback, Guid correlationId = default(Guid))
            where TAppEvent : IApplicationEvent;


        /// <summary>
        ///     Unsubscribes (or removes) an event subscription from the EventAggregator
        /// </summary>
        /// <typeparam name="TAppEvent">
        ///     The Event Message type that is to be published. This must implement an interface for
        ///     <see cref="IApplicationEvent" />
        /// </typeparam>
        /// <param name="subscription">Subscription to remove</param>
        void UnSubscribe<TAppEvent>(ISubscription<TAppEvent> subscription) where TAppEvent : IApplicationEvent;


        /// <summary>
        ///     Clears all subscriptions from the EventAggregator leaving nothing behind.
        /// </summary>
        void ClearAllSubscriptions();


        /// <summary>
        ///     Clears all subscriptions for each <see cref="Type" /> given in the array parameter.
        /// </summary>
        /// <param name="exceptMessages">Array of subscription types from which to remove subscriptions</param>
        void ClearAllSubscriptions(Type[] exceptMessages);


        /// <summary>
        ///     Clears all subscriptions for a given event message type
        /// </summary>
        /// <typeparam name="T">Type of event from which to remove subscriptions</typeparam>
        void ClearSubscriptionsFor<T>();


        /// <summary>
        ///     Clears all subscriptions that have the given <see cref="correlationId" />
        /// </summary>
        /// <param name="correlationId">Correlation Id used to find subscriptions inside the EventAggregator</param>
>>>>>>> dev
        void ClearAllCorrelatedSubscriptions(Guid correlationId);
    }
}