using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class NotifyResultChangedWeakSubscription<TSource, TTarget> : EventPatternWeakSubscription<TSource, NotifyEventArgs, TTarget>
        where TTarget : class
        where TSource : class, INotifyResultChanged
    {
        public NotifyResultChangedWeakSubscription(TSource source, TTarget target)
            : base(source, target)
        { }

        protected override sealed void Subscribe(EventHandler<NotifyEventArgs> handler) => Source.ResultChanged += handler;
        protected override void Unsubscribe(EventHandler<NotifyEventArgs> handler) => Source.ResultChanged -= handler;
    }
}
