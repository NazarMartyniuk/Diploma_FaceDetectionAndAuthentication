using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public readonly struct ReferenceKey : IEquatable<ReferenceKey>
    {
        public readonly object Value;

        public ReferenceKey(object value)
            => Value = value;

        public bool Equals(ReferenceKey other)
            => ReferenceEquals(Value, other.Value);

        public override int GetHashCode()
            => RuntimeHelpers.GetHashCode(Value);

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is ReferenceKey rk) return Equals(rk);
            return false;
        }
    }
}
