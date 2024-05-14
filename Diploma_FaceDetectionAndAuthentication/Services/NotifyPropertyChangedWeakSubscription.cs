using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class NotifyPropertyChangedWeakSubscription<TSource, TTarget> : WeakReference<TTarget>, ISubscription
        where TTarget : class
        where TSource : class, INotifyPropertyChanged
    {
        private readonly PropertyChangedEventHandler _EventHandler;
        public TSource Source { get; }

        public NotifyPropertyChangedWeakSubscription(TSource source, TTarget target)
            : base(target)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            _EventHandler = SourcePropertyChanged;
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (TryGetTarget(out var target))
            {
                OnPropertyChanged(target, sender, e);
            }
            else
            {
                Unsubscribe();
            }
        }

        protected abstract void OnPropertyChanged(TTarget target, object sender, PropertyChangedEventArgs e);

        public void Subscribe() => Source.PropertyChanged += _EventHandler;

        public void Unsubscribe() => Source.PropertyChanged -= _EventHandler;
    }
}
