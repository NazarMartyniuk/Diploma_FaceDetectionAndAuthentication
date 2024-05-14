using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public abstract class NotifyEventArgs : EventArgs
    {
        private readonly object _Source;
        public object Source { get { return _Source; } }

        protected NotifyEventArgs(object source)
        {
            _Source = source;
        }

        public static NotifyEventArgs<TArgs> Create<TArgs>(object source, TArgs args)
            where TArgs : EventArgs
        {
            return new NotifyEventArgs<TArgs>(source, args);
        }
    }

    public class NotifyEventArgs<TArgs> : NotifyEventArgs
        where TArgs : EventArgs
    {
        private readonly TArgs _Args;
        public TArgs Args { get { return _Args; } }

        public NotifyEventArgs(object source, TArgs args)
            : base(source)
        {
            _Args = args;
        }
    }
}
