using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    internal sealed class ChildObserver : NotifyObserver
    {
        private sealed class WeakSubscription : NotifyResultChangedWeakSubscription<NotifyContext, ChildObserver>
        {
            public WeakSubscription(NotifyContext context, ChildObserver target)
                : base(context, target) { }

            protected override void RaiseChanges(ChildObserver target, object sender, NotifyEventArgs args)
                => target.OnResultChanged(sender, args);
        }

        private readonly WeakSubscription _WeakSubscription;
        public override object Source => _WeakSubscription.Source;

        public ChildObserver(NotifyContext childObserver)
        {
            if (childObserver == null) throw new ArgumentNullException(nameof(childObserver));

            _WeakSubscription = new WeakSubscription(childObserver, this);
        }

        private void OnResultChanged(object sender, NotifyEventArgs e) => OnProviderChanged(e);

        protected override void ReInit() { }

        protected override void OnSubscribe() => _WeakSubscription.Subscribe();
        protected override void OnUnsubscribe() => _WeakSubscription.Unsubscribe();
    }
}
