using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class NotifyCollectionChangedWeakSubscription<TSource, TTarget> : WeakReference<TTarget>, ISubscription
        where TTarget : class
        where TSource : class, INotifyCollectionChanged
    {
        private readonly NotifyCollectionChangedEventHandler _EventHandler;
        public TSource Source { get; }

        public NotifyCollectionChangedWeakSubscription(TSource source, TTarget target)
            : base(target)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            _EventHandler = SourceCollectionChanged;
        }

        protected abstract void OnCollectionChanged(TTarget target, object sender, NotifyCollectionChangedEventArgs e);

        public void Subscribe() => Source.CollectionChanged += _EventHandler;
        public void Unsubscribe() => Source.CollectionChanged -= _EventHandler;

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TryGetTarget(out var target))
            {
                OnCollectionChanged(target, sender, e);
            }
            else
            {
                Unsubscribe();
            }
        }
    }
}
