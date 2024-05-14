using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class WeakReference<T> : WeakReference
        where T : class
    {
        public WeakReference(T target)
            : base(target) { }

        public virtual new T Target
        {
            get => (T)base.Target;
            protected set => base.Target = value;
        }

        public bool TryGetTarget(out T target)
        {
            var ot = base.Target;
            if (IsAlive)
            {
                target = (T)ot;
                return true;
            }

            target = null;
            return false;
        }
    }
}
