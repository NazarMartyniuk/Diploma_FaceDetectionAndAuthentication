using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public interface IFunc<T1, T2, TResult>
    {
        TResult Invoke(T1 arg1, T2 arg2);
    }
}
