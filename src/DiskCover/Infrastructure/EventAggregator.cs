// ----------------------------------------------------
// 
// AIMP DotNet SDK
// 
// Copyright (c) 2014 - 2020 Evgeniy Bogdan
// https://github.com/martin211/aimp_dotnet
// 
// Mail: mail4evgeniy@gmail.com
// 
// ----------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AIMP.DiskCover.CoverFinder;

namespace AIMP.DiskCover.Infrastructure
{
    public interface IConcurrentList : IEnumerable
    {
        int Count { get; }
        void Add(object o);

        void Remove(object o);

        void Clear();
    }

    /// <summary>
    /// Interface IEventAggregator
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        void Clear<TEvent>() where TEvent : EventArgs;

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Raises the specified ev.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        /// <param name="ev">The ev.</param>
        void Raise<TEvent>(TEvent ev) where TEvent : EventArgs;

        /// <summary>
        /// Raises the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        /// <param name="ev">The ev.</param>
        void RaiseAsync<TEvent>(TEvent ev) where TEvent : EventArgs;

        /// <summary>
        /// Registers the specified event action.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        /// <param name="eventAction">The event action.</param>
        /// <param name="context">The context.</param>
        void Register<TEvent>(Action<TEvent> eventAction, SynchronizationContext context = null) where TEvent : EventArgs;

        /// <summary>
        /// Unregisters the specified event action.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        /// <param name="eventAction">The event action.</param>
        /// <param name="context">The context.</param>
        void Unregister<TEvent>(Action<TEvent> eventAction, SynchronizationContext context = null) where TEvent : EventArgs;
    }

    internal class ConcurrentList<T> : IConcurrentList
    {
        private readonly ConcurrentDictionary<T, bool> _dictionary = new ConcurrentDictionary<T, bool>();

        public int Count => _dictionary.Count;

        public void Add(object o)
        {
            if (!(o is T index))
            {
                return;
            }

            _dictionary[index] = true;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }

        public void Remove(object o)
        {
            if (!(o is T key))
            {
                return;
            }

            _dictionary.TryRemove(key, out _);
        }
    }
    
    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, IConcurrentList> _listenerMap =
            new ConcurrentDictionary<Type, IConcurrentList>();

        private readonly object _syncLock = new object();

        public void Register<TEvent>(Action<TEvent> eventAction, SynchronizationContext context = null) where TEvent : EventArgs
        {
            IssueSyncLock(() =>
            {
                var concurrentList = GetListeners(typeof(TEvent));
                if (concurrentList == null)
                {
                    concurrentList = new ConcurrentList<EventActionContext<TEvent>>();
                    _listenerMap[typeof(TEvent)] = concurrentList;
                }

                if (GetListener(eventAction, context) != null)
                {
                    return;
                }

                concurrentList.Add(new EventActionContext<TEvent>(eventAction, context));
            });
        }

        public void Raise<TEvent>(TEvent ev) where TEvent : EventArgs
        {
            var listeners = GetListeners(typeof(TEvent));
            if (listeners == null)
            {
                return;
            }

            foreach (var obj in listeners)
            {
                var eventActionListener = obj as EventActionContext<TEvent>;
                var synchronizationContext = eventActionListener?.Context ?? SynchronizationContext.Current;
                if (synchronizationContext != null)
                {
                    try
                    {
                        synchronizationContext.Send(o =>
                        {
                            var eventActionContext = eventActionListener;
                            if (eventActionContext == null)
                            {
                                return;
                            }
                            System.Diagnostics.Debug.WriteLine($"Raise event: {ev.GetType().FullName}");
                            eventActionContext.EventAction(ev);
                        }, null);
                    }
                    catch (InvalidAsynchronousStateException)
                    {
                    }
                }
                else
                {
                    var eventActionContext = eventActionListener;
                    eventActionContext?.EventAction(ev);
                }
            }
        }

        public void RaiseAsync<TEvent>(TEvent ev) where TEvent : EventArgs
        {
            var listeners = GetListeners(typeof(TEvent));
            if (listeners == null)
            {
                return;
            }

            foreach (var obj in listeners)
            {
                var eventActionListener = obj as EventActionContext<TEvent>;
                var synchronizationContext = eventActionListener?.Context ?? SynchronizationContext.Current;
                if (synchronizationContext != null)
                {
                    try
                    {
                        synchronizationContext.Post(o =>
                        {
                            var eventActionContext = eventActionListener;
                            if (eventActionContext == null)
                            {
                                return;
                            }

                            eventActionContext.EventAction(ev);
                        }, null);
                    }
                    catch (InvalidAsynchronousStateException)
                    {
                    }
                }
                else
                {
                    var eventActionContext = eventActionListener;
                    eventActionContext?.EventAction(ev);
                }
            }
        }

        public void Unregister<TEvent>(Action<TEvent> eventAction, SynchronizationContext context = null) where TEvent : EventArgs
        {
            IssueSyncLock(() =>
            {
                var listeners = GetListeners(typeof(TEvent));
                var listener1 = GetListener(eventAction, context);
                if (listener1 == null)
                {
                    return;
                }

                listeners.Remove(listener1);
                var listener2 = _listenerMap[typeof(TEvent)];
                if ((listener2 != null ? listener2.Count == 0 ? 1 : 0 : 0) == 0)
                {
                    return;
                }

                _listenerMap.TryRemove(typeof(TEvent), out _);
            });
        }

        public void Clear<TEvent>() where TEvent : EventArgs
        {
            if (!_listenerMap.ContainsKey(typeof(TEvent)))
            {
                return;
            }

            _listenerMap[typeof(TEvent)]?.Clear();
        }

        public void Clear()
        {
            _listenerMap?.Clear();
        }

        private void IssueSyncLock(Action action)
        {
            lock (_syncLock)
            {
                action();
            }
        }

        private IConcurrentList GetListeners(Type eventType)
        {
            IConcurrentList concurrentList;
            return _listenerMap.TryGetValue(eventType, out concurrentList) ? concurrentList : null;
        }

        private EventActionContext<TEvent> GetListener<TEvent>(
            Action<TEvent> eventAction,
            SynchronizationContext context = null)
        {
            var compare = new EventActionContext<TEvent>(eventAction, context);
            return GetListeners(typeof(TEvent)).Cast<object>()
                .Select(listener => listener as EventActionContext<TEvent>).FirstOrDefault(registeredListener =>
                    registeredListener?.Context == compare.Context &&
                    registeredListener?.EventAction == compare.EventAction);
        }
    }
}
