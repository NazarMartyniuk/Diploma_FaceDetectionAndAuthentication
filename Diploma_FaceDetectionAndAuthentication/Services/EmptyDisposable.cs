using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class EmptyDisposable : IDisposable
    {
        public static readonly IDisposable Empty = new EmptyDisposable();

        private EmptyDisposable() { }

        public void Dispose() { }
    }
}
