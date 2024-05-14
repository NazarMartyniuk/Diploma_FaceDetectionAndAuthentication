using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class LazyEvents
    {
        private class EditMode : IDisposable
        {
            private readonly LazyEvents _Owner;

            public EditMode(LazyEvents owner)
            {
                _Owner = owner;
            }

            public void Dispose()
            {
                _Owner.EndEdit(this);
            }
        }

        private class IgnoreSetterFromUI : IDisposable
        {
            private readonly LazyEvents _Owner;
            public IgnoreSetterFromUI(LazyEvents owner)
            {
                _Owner = owner;
                _Owner._IgnoreSetterFromUICounter++;
            }

            public void Dispose()
            {
                _Owner._IgnoreSetterFromUICounter--;
            }
        }

        private class IgnoreSetter : IDisposable
        {
            private readonly LazyEvents _Owner;
            public IgnoreSetter(LazyEvents owner)
            {
                _Owner = owner;
                _Owner._IgnoreSetterCounter++;
            }

            public void Dispose()
            {
                _Owner._IgnoreSetterCounter--;
            }
        }

        private int _IgnoreSetterFromUICounter = 0;
        private int _IgnoreSetterCounter = 0;

        private static readonly LazyEvents _Current = new LazyEvents();

        private readonly Dictionary<object, Action> _RaiseEvents =
            new Dictionary<object, Action>();
        private readonly Stack<EditMode> _EditStack =
            new Stack<EditMode>();

        protected LazyEvents() { }

        protected bool IsEditing { get { return _EditStack.Count > 0; } }

        private void AddEvent<TKey>(TKey key, Action raise)
        {
            if (!_RaiseEvents.ContainsKey(key))
            {
                _RaiseEvents.Add(key, raise);
            }
        }

        protected void RaiseEvent<TKey>(TKey key, Action raise)
        {
            if (_EditStack.Count == 0)
            {
                if (_IgnoreSetterCounter > 0)
                {
                    using (new IgnoreSetterFromUI(this))
                    {
                        raise();
                    }
                }
                else
                {
                    raise();
                }
            }
            else
            {
                if (_IgnoreSetterCounter > 0)
                {
                    var old = raise;
                    raise = () =>
                    {
                        using (new IgnoreSetterFromUI(this))
                        {
                            old();
                        }
                    };
                }

                this.AddEvent(key, raise);
            }
        }

        public virtual IDisposable BeginIgnoreSetterMode()
        {
            if (!NotifyContext.IsStartingThread) return EmptyDisposable.Empty;

            return new IgnoreSetter(this);
        }

        public virtual IDisposable BeginEditMode()
        {
            if (!NotifyContext.IsStartingThread) return EmptyDisposable.Empty;

            EditMode result = new EditMode(this);
            _EditStack.Push(result);
            return result;
        }

        private void EndEdit(EditMode editMode)
        {
            if (_EditStack.Pop() != editMode)
            {
                throw new InvalidOperationException();
            }
            else if ((_EditStack.Count == 0) && (_RaiseEvents.Count > 0))
            {
                var actions = _RaiseEvents.Values.ToArray();
                _RaiseEvents.Clear();

                foreach (var a in actions)
                {
                    a();
                }
            }
        }

        public static IDisposable BeginEdit()
        {
            if (!NotifyContext.IsStartingThread) return EmptyDisposable.Empty;

            return _Current.BeginEditMode();
        }

        public static IDisposable BeginIgnoreSetter()
        {
            if (!NotifyContext.IsStartingThread) return EmptyDisposable.Empty;

            return _Current.BeginIgnoreSetterMode();
        }

        public static bool CanSetPropertyFromUI
        {
            get { return NotifyContext.IsStartingThread && _Current._IgnoreSetterFromUICounter == 0; }
        }

        public static void Raise<TKey>(TKey key, Action raise)
        {
            _Current.RaiseEvent(key, raise);
        }

        public static void Raise<TKey, TEvent>(TKey key, Func<TEvent> getEventHandler, Action<TEvent> raise)
        {
            _Current.RaiseEvent(key, () => ToRaise(getEventHandler, raise));
        }

        private static void ToRaise<TEvent>(Func<TEvent> getEventHandler, Action<TEvent> raise)
        {
            var ev = getEventHandler();
            if (ev != null)
            {
                raise(ev);
            }
        }
    }
}
