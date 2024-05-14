using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class EventPatternWeakSubscription<TSource, TEventArgs, TTarget> : WeakReference<TTarget>, ISubscription
        where TTarget : class
        where TSource : class
        where TEventArgs : EventArgs
    {
        private readonly EventHandler<TEventArgs> _EventHandler;
        public TSource Source { get; }
        public EventPatternWeakSubscription(TSource source, TTarget target)
            : base(target)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));

            _EventHandler = Handler;
        }

        private void Handler(object sender, TEventArgs e)
        {
            if (TryGetTarget(out var target))
            {
                RaiseChanges(target, Source, e);
            }
            else
            {
                Unsubscribe();
            }
        }

        protected abstract void RaiseChanges(TTarget target, object sender, TEventArgs args);
        protected abstract void Subscribe(EventHandler<TEventArgs> handler);
        protected abstract void Unsubscribe(EventHandler<TEventArgs> handler);

        public void Subscribe() => Subscribe(_EventHandler);
        public void Unsubscribe() => Unsubscribe(_EventHandler);
    }
}
