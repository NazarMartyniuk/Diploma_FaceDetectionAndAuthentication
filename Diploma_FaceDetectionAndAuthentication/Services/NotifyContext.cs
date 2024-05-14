using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class NotifyContext : IDisposable, INotifyResultChanged
    {
        internal class DependenciesCollectorContext : IDisposable
        {
            private class IgnoreSubscriber : IDisposable
            {
                private readonly DependenciesCollectorContext _Current;
                public IgnoreSubscriber()
                {
                    _Current = DependenciesCollectorContext.CurrentContext;
                    DependenciesCollectorContext.CurrentContext = null;
                }

                public void Dispose()
                {
                    DependenciesCollectorContext.CurrentContext = _Current;
                }
            }

            [ThreadStatic]
            private static DependenciesCollectorContext _CurrentContext;
            internal static DependenciesCollectorContext CurrentContext
            {
                get { return _CurrentContext; }
                private set { _CurrentContext = value; }
            }

            public static void Subscribe<TSource, TValue>(TSource source, TValue value, string propertyName)
                where TSource : class, INotifyPropertyChanged
            {
                var currentContext = CurrentContext;
                if (currentContext != null)
                {
                    currentContext.SubscribeNotifyPropertyChanged(source, propertyName);
                    if ((value != null) && (value is INotifyCollectionChanged))
                    {
                        currentContext.SubscribeNotifyCollectionChanged((INotifyCollectionChanged)value);
                    }
                }
            }

            internal static void PushDependencyFromCollection(INotifyCollectionChanged source)
                => CurrentContext?.SubscribeNotifyCollectionChanged(source);

            public static void SubscribeProperty<TSource>(TSource source, string propertyName)
                where TSource : class, INotifyPropertyChanged
                => CurrentContext?.SubscribeNotifyPropertyChanged(source, propertyName);

            public static void SubscribeContextAsChild(NotifyContext context)
                => CurrentContext?.SubscribeAsChild(context);

            public static IDisposable IgnoreChildChanges()
            {
                return new IgnoreSubscriber();
            }

            private DependenciesCollectorContext _ParentSubscribeContext;

            private readonly NotifyContext _Context;
            private bool _IsChangedSource = false;

            public DependenciesCollectorContext(NotifyContext context)
            {
                _Context = context;
                _Context._CurrentSubscriber = this;

                _ParentSubscribeContext = CurrentContext;
                CurrentContext = this;
                this.SubscribeThisAsChild();
            }

            public void SubscribeNotifyPropertyChanged<T>(T source, string propertyName)
                where T : class, INotifyPropertyChanged
                => (
                    (NotifyPropertyChangedObserver)this.ToSubscribe(source, s => new NotifyPropertyChangedObserver(s))
                )?.SubscribeProperty(propertyName);

            public void SubscribeNotifyCollectionChanged<T>(T collection)
                where T : class, INotifyCollectionChanged
            {
                this.ToSubscribe(new { collection },
                    s => new NotifyCollectionChangedObserver<T>(s.collection));
            }

            public void SubscribeAsChild(NotifyContext childContext)
            {
                this.ToSubscribe(childContext, ch => new ChildObserver(ch));
            }

            private void SubscribeThisAsChild()
            {
                if (_ParentSubscribeContext != null)
                {
                    _ParentSubscribeContext.SubscribeAsChild(_Context);
                }
            }

            private NotifyObserver ToSubscribe<TKey>(TKey key,
                Func<TKey, NotifyObserver> createNew)
                where TKey : class
            {
                if (_IsChangedSource)
                {
                    return null;
                }

                return _Context.ToSubscribe(key, createNew);
            }

            public void PrevisiousDataChanged()
            {
                _IsChangedSource = true;
            }

            public void Dispose()
            {
                CurrentContext = _ParentSubscribeContext;
                _Context._CurrentSubscriber = null;
                _ParentSubscribeContext = null;
                _Context.UnSubscribePrevisious();
            }
        }

        private readonly struct NotifyObserverWrapper
        {
            public readonly NotifyObserver Value;
            public NotifyObserverWrapper(NotifyObserver value) => Value = value;
        }

        private readonly struct Unsubscriber : IFunc<ReferenceKey, NotifyObserverWrapper, bool>
        {
            private readonly EventHandler<NotifyEventArgs> _delegate;
            public Unsubscriber(EventHandler<NotifyEventArgs> d) => _delegate = d;
            public bool Invoke(ReferenceKey arg1, NotifyObserverWrapper arg2)
            {
                arg2.Value.ResultChanged -= _delegate;
                return true;
            }
        }

        private readonly struct UnSubscriberFromPrevisious : IFunc<ReferenceKey, NotifyObserverWrapper, bool>
        {
            private readonly EventHandler<NotifyEventArgs> _delegate;
            private readonly int _subscriptionStep;
            public UnSubscriberFromPrevisious(EventHandler<NotifyEventArgs> d, int subscriptionStep)
            {
                _delegate = d;
                _subscriptionStep = subscriptionStep;
            }

            public bool Invoke(ReferenceKey arg1, NotifyObserverWrapper arg2)
            {
                if (arg2.Value.IsEmptyFor(_subscriptionStep))
                {
                    arg2.Value.ResultChanged -= _delegate;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private DependenciesCollection<ReferenceKey, NotifyObserverWrapper> _Observers
            = DependenciesCollection<ReferenceKey, NotifyObserverWrapper>.Create();
        private int _SubscriptionStep = 0;

        private DependenciesCollectorContext _CurrentSubscriber;
        private static readonly int? _StartingManagedThreadId;
        internal static bool IsStartingThread
        {
            get { return _StartingManagedThreadId.Value == Thread.CurrentThread.ManagedThreadId; }
        }

        static NotifyContext()
        {
            _StartingManagedThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public NotifyContext() { }

        private NotifyObserver ToSubscribe<TKey>(TKey key, Func<TKey, NotifyObserver> createNew)
            where TKey : class
        {
            ref var slot = ref _Observers.GetOrRegisterSlotFor(new ReferenceKey(key), out var isFree);
            NotifyObserver result;
            if (isFree)
            {
                result = createNew(key);
                slot = new NotifyObserverWrapper(result);
                result.ResultChanged += SourceChanged;
            }
            else
            {
                result = slot.Value;
            }
            result.ReInit(_SubscriptionStep);

            return result;
        }

        public IDisposable ReSubscribe(bool isIgnoreRecursive = false)
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;

            var notifier = this;
            if (_CurrentSubscriber != null)
            {
                if (isIgnoreRecursive)
                {
                    notifier = new NotifyContext();
                    return notifier.ReSubscribe(isIgnoreRecursive);
                }
                else
                {
                    throw new InvalidOperationException("Recursive error");
                }
            }

            _SubscriptionStep++;
            return new DependenciesCollectorContext(notifier);
        }

        protected void ReSubscribeAsChild()
        {
            if (!IsStartingThread) return;

            DependenciesCollectorContext.SubscribeContextAsChild(this);
        }

        private void UnSubscribePrevisious()
            => _Observers.Remove(new UnSubscriberFromPrevisious(SourceChanged, _SubscriptionStep));

        private void UnSubscribe()
            => _Observers.Remove(new Unsubscriber(SourceChanged));

        private void SourceChanged(object sender, NotifyEventArgs e)
        {
            if (_CurrentSubscriber != null)
            {
                _CurrentSubscriber.PrevisiousDataChanged();
            }

            this.OnResultChanged(e);
        }

        protected virtual void OnResultChanged(NotifyEventArgs args)
        {
            if (!IsStartingThread) return;

            LazyEvents.Raise(new { Source = this, Args = args }, () => this.RaiseResultChanged(args));
        }

        private void RaiseResultChanged(NotifyEventArgs args)
        {
            if (!IsStartingThread) return;

            var handler = _ResultChanged;
            if (handler == null) return;

            handler(this, args);
        }

        private EventHandler<NotifyEventArgs> _ResultChanged;
        public event EventHandler<NotifyEventArgs> ResultChanged
        {
            add
            {
                if (!IsStartingThread) return;
                _ResultChanged += value;
            }
            remove
            {
                if (!IsStartingThread) return;
                _ResultChanged -= value;
            }
        }

        protected bool HasEmptySubscribers
        {
            get
            {
                return !IsStartingThread || _ResultChanged == null;
            }
        }

        public void Dispose()
        {
            this.UnSubscribe();
        }

        internal static bool IsIgnoredChildChanges
        {
            get { return DependenciesCollectorContext.CurrentContext == null; }
        }

        internal static IDisposable IgnoreChildChanges()
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;

            return DependenciesCollectorContext.IgnoreChildChanges();
        }
    }
}
