﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public interface ISubscription
    {
        void Subscribe();
        void Unsubscribe();
    }
}
