using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    internal abstract class NotifyObserver : INotifyResultChanged
    {
        private int _subscriptionStep;
        public abstract object Source { get; }
        public bool IsEmptyFor(int subscriptionStep) => _subscriptionStep != subscriptionStep;

        internal void ReInit(int subscriptionStep)
        {
            if (_subscriptionStep !=  subscriptionStep)
            {
                _subscriptionStep = subscriptionStep;
                ReInit();
            }
        }

        protected abstract void ReInit();

        protected virtual void OnSourceChanged(NotifyEventArgs e)
            => _ResultChanged?.Invoke(this, e);

        private EventHandler<NotifyEventArgs> _ResultChanged;
        public event EventHandler<NotifyEventArgs> ResultChanged
        {
            add
            {
                bool canSubscribe = _ResultChanged == null;
                _ResultChanged += value;
                if (canSubscribe)
                {
                    this.OnSubscribe();
                }
            }
            remove
            {
                _ResultChanged -= value;
                if (_ResultChanged == null)
                {
                    this.OnUnsubscribe();
                }
            }
        }

        protected void OnProviderChanged(NotifyEventArgs e)
            => OnSourceChanged(e);

        protected abstract void OnSubscribe();
        protected abstract void OnUnsubscribe();

        public virtual void Dispose()
        {
            _ResultChanged = null;
            OnUnsubscribe();
        }
    }
}
