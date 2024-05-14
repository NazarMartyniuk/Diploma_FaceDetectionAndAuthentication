using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged, IDisposable
    {
        private readonly IDictionary<string, IDisposable> _Methods = new Dictionary<string, IDisposable>();

        private static bool IsStartingThread
        {
            get { return NotifyContext.IsStartingThread; }
        }

        static NotifyPropertyChanged()
        { }

        private long _UniqueObjectId;
        protected long UniqueObjectId => _UniqueObjectId;

        protected NotifyPropertyChanged() { }

        protected TResult Subscribe<TResult>(Func<TResult> method, bool isIgnoreRecursive = false,
            [CallerMemberName] string propertyName = null)
        {
            if (!IsStartingThread) return method();

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is null or empty", "propertyName");
            }

            IDisposable disposable;
            NotifyMethod<TResult> nMethod;
            if (_Methods.TryGetValue(propertyName, out disposable))
            {
                nMethod = (NotifyMethod<TResult>)disposable;
            }
            else
            {
                nMethod = new NotifyMethod<TResult>(method, isIgnoreRecursive);
                _Methods.Add(propertyName, nMethod);
                nMethod.ResultChanged += (s, e) => this.OnPropertyChangedFromSubscriber(propertyName, e);
            }

            using (this.Subscribe(propertyName + "<NotifyMethod>", isIgnoreRecursive))
            {
                return nMethod.Result;
            }
        }

        protected IDisposable IgnoreChildChanges()
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;

            return NotifyContext.IgnoreChildChanges();
        }

        protected IDisposable Subscribe([CallerMemberName] string propertyName = null, bool isIgnoreRecursive = false) 
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;
            if (_PropertyChanged == null && NotifyContext.IsIgnoredChildChanges) return EmptyDisposable.Empty;

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName");
            }

            this.SubscribeProperty(propertyName);
            IDisposable disposable;
            if (_Methods.TryGetValue(propertyName, out disposable))
            {
                return ((NotifyContext)disposable).ReSubscribe(isIgnoreRecursive);
            }
            else
            {
                var context = new NotifyContext();
                _Methods.Add(propertyName, context);
                context.ResultChanged += (s, e) => this.OnPropertyChangedFromSubscriber(propertyName, e);
                return context.ReSubscribe(isIgnoreRecursive);
            }
        }

        protected bool CanSetPropertyFromUI { get { return LazyEvents.CanSetPropertyFromUI; } }

        public IDisposable BeginIgnoreSetter()
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;

            return LazyEvents.BeginIgnoreSetter();
        }

        public IDisposable BeginEditMode()
        {
            if (!IsStartingThread) return EmptyDisposable.Empty;

            return LazyEvents.BeginEdit();
        }

        protected virtual void OnDisposing() { }

        public void Dispose()
        {
            this.OnDisposing();
            foreach (var m in _Methods)
            {
                m.Value.Dispose();
            }
        }

        protected void OnPropertyChangedFromSubscriber(string subscribedProperty, NotifyEventArgs e)
        {
            if (!IsStartingThread) return;

            if (e.Source == this)
            {
                var eg = e as NotifyEventArgs<PropertyChangedEventArgs>;
                if ((eg != null) && (eg.Args.PropertyName == subscribedProperty))
                {
                    return;
                }
            }

            this.OnPropertyChanged(subscribedProperty);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!IsStartingThread) return;

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (_PropertyChanged != null)
            {
                Action raise = () => _PropertyChanged.RaisePropertyChanged(this, propertyName);
                LazyEvents.Raise(new { Source = this, PropertyName = propertyName }, raise);
            }
        }

        private PropertyChangedEventHandler _PropertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (!IsStartingThread) return;

                _PropertyChanged += value;
            }
            remove
            {
                _PropertyChanged -= value;
            }
        }

        protected virtual bool SetProperty<T>(ref T variable, T value, [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref variable, value, EqualityComparer<T>.Default, propertyName);
        }

        protected bool SetProperty<T>(ref T variable, T value, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (comparer == null) throw new ArgumentNullException("comparer");

            if (comparer.Equals(variable, value)) return false;

            variable = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
