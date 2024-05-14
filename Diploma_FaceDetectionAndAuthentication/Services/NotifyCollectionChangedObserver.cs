using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    internal sealed class NotifyCollectionChangedObserver<TSource> : NotifyObserver
        where TSource : class, INotifyCollectionChanged
    {
        private sealed class WeakSubscription : NotifyCollectionChangedWeakSubscription<TSource, NotifyCollectionChangedObserver<TSource>>
        {
            public WeakSubscription(TSource source, NotifyCollectionChangedObserver<TSource> target)
                : base(source, target) { }

            protected override void OnCollectionChanged(NotifyCollectionChangedObserver<TSource> target, object sender, NotifyCollectionChangedEventArgs e)
                => target.OnCollectionChanged(sender, e);
        }

        private readonly WeakSubscription _WeakObserver;
        public override object Source => _WeakObserver.Source;

        public NotifyCollectionChangedObserver(TSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _WeakObserver = new WeakSubscription(source, this);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => OnProviderChanged(NotifyEventArgs.Create(sender, e));

        protected override void ReInit() { }

        protected override void OnSubscribe() => _WeakObserver.Subscribe();
        protected override void OnUnsubscribe() => _WeakObserver.Unsubscribe();
    }
}
