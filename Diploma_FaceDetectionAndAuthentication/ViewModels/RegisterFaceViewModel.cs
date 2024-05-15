using Diploma_FaceDetectionAndAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.ViewModels
{
    public class RegisterFaceViewModel
    {
        private readonly User _User;

        public RegisterFaceViewModel(User user)
        {
            _User = user;
        }
    }
}
