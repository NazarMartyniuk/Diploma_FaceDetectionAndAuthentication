using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    internal class NotifyPropertyChangedObserver : NotifyObserver
    {
        private sealed class WeakSubscription : NotifyPropertyChangedWeakSubscription<INotifyPropertyChanged, NotifyPropertyChangedObserver>
        {
            public WeakSubscription(INotifyPropertyChanged source, NotifyPropertyChangedObserver target)
                : base(source, target) { }

            protected override void OnPropertyChanged(NotifyPropertyChangedObserver target, object sender, PropertyChangedEventArgs e)
                => target.OnPropertyChanged(sender, e);
        }

        private readonly WeakSubscription _WeakSubscription;
        public override object Source => _WeakSubscription.Source;
        private readonly HashSet<string> _Properties = new HashSet<string>();

        public NotifyPropertyChangedObserver(INotifyPropertyChanged source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _WeakSubscription = new WeakSubscription(source, this);
        }

        public void SubscribeProperty(string propertyName)
            => _Properties.Add(propertyName);

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_Properties.Contains(e.PropertyName))
            {
                this.OnProviderChanged(NotifyEventArgs.Create(sender, e));
            }
        }

        protected override void ReInit()
        {
            _Properties.Clear();
        }

        protected override void OnSubscribe() => _WeakSubscription.Subscribe();
        protected override void OnUnsubscribe() => _WeakSubscription.Unsubscribe();

        public override string ToString()
        {
            if (_Properties.Count > 0)
            {
                using (var en = _Properties.GetEnumerator())
                {
                    en.MoveNext();
                    string result = "Observer to properties: {" + en.Current;
                    while (en.MoveNext())
                    {
                        result += "; " + en.Current;
                    }

                    result += "}";
                    return result;
                }
            }
            else
            {
                return "<Empty>";
            }
        }
    }
}
