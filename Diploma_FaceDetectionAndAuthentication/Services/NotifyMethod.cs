using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class NotifyMethod<TResult> : IDisposable, INotifyResultChanged
    {
        private class ContextObserver : NotifyContext
        {
            private readonly bool _IsIgnoreRecursive;
            private readonly Func<TResult> _Function;
            private readonly NotifyMethod<TResult> _Owner;

            private bool _IsFirstInit = true;

            private TResult _CurrentResult;
            private bool _IsChangedSource = true;
            private NotifyEventArgs _PrevArgs;

            public ContextObserver(
                NotifyMethod<TResult> owner,
                Func<TResult> function,
                bool isIgnoreRecursive = false)
                : base()
            {
                _Owner = owner;
                _IsIgnoreRecursive = isIgnoreRecursive;
                _Function = function;
            }

            private bool _CanAutoRecalculate;
            public bool CanAutoRecalculate
            {
                get { return _CanAutoRecalculate; }
                set
                {
                    if (_CanAutoRecalculate != value)
                    {
                        _CanAutoRecalculate = value;
                        if (_CanAutoRecalculate)
                        {
                            this.CalculateResult(_IsFirstInit);
                        }
                    }
                }
            }

            public TResult Result
            {
                get
                {
                    if (!IsStartingThread) return _Function();
                    if (IsIgnoredChildChanges)
                    {
                        return _CurrentResult = RecalculateResult();
                    }

                    this.CalculateResult(_IsFirstInit);
                    this.ReSubscribeAsChild();

                    return _CurrentResult;
                }
            }

            private TResult RecalculateResult()
            {
                using (this.ReSubscribe(_IsIgnoreRecursive))
                {
                    return _Function();
                }
            }

            private void CalculateResult(bool isFirstInit)
            {
                if (_IsChangedSource)
                {
                    _IsFirstInit = false;
                    _IsChangedSource = false;

                    var newValue = RecalculateResult();
                    bool isRaiseChangeResult = !isFirstInit && !EqualityComparer<TResult>.Default.Equals(newValue, _CurrentResult);
                    _CurrentResult = newValue;

                    if (isRaiseChangeResult)
                    {
                        this.RaiseChangeResult(_PrevArgs);
                    }
                }
            }

            private void RaiseChangeResult(NotifyEventArgs e)
            {
                if (!IsStartingThread) return;

                base.OnResultChanged(e);
                _Owner.OnResultChanged(e);
            }

            protected override void OnResultChanged(NotifyEventArgs e)
            {
                if (!IsStartingThread) return;

                _IsChangedSource = true;
                _PrevArgs = e;
                if (!base.HasEmptySubscribers || this.CanAutoRecalculate)
                {
                    this.CalculateResult(_IsFirstInit);
                }
            }
        }

        private readonly ContextObserver _Observer;

        public NotifyMethod(Func<TResult> function,
            bool isIgnoreRecursive = false)
        {
            _Observer = new ContextObserver(this, function, isIgnoreRecursive);
        }

        public TResult Result { get { return _Observer.Result; } }
        public bool CanAutoRecalculateResult
        {
            get { return _Observer.CanAutoRecalculate; }
            set { _Observer.CanAutoRecalculate = value; }
        }

        protected virtual void OnResultChanged(NotifyEventArgs args)
        {
            if (!NotifyContext.IsStartingThread) return;

            if (_ResultChanged != null)
            {
                LazyEvents.Raise(new { Source = this, Args = args }, () => this.RaiseResultChanged(args));
            }
        }

        private void RaiseResultChanged(NotifyEventArgs e)
        {
            if (!NotifyContext.IsStartingThread) return;

            if (_ResultChanged != null)
            {
                _ResultChanged(this, e);
            }
        }

        private EventHandler<NotifyEventArgs> _ResultChanged;
        public event EventHandler<NotifyEventArgs> ResultChanged
        {
            add
            {
                if (!NotifyContext.IsStartingThread) return;

                _ResultChanged += value;
                _Observer.CanAutoRecalculate = true;
            }
            remove
            {
                if (!NotifyContext.IsStartingThread) return;

                _ResultChanged -= value;
                _Observer.CanAutoRecalculate = _ResultChanged != null;
            }
        }

        public void Dispose()
        {
            _Observer.Dispose();
        }
    }
}
