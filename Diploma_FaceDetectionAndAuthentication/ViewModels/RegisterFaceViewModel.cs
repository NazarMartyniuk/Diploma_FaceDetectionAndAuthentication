using Diploma_FaceDetectionAndAuthentication.Models;
using Diploma_FaceDetectionAndAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.ViewModels
{
    public class RegisterFaceViewModel : NotifyPropertyChanged
    {
        public User _User { get; set; }

        public RegisterFaceViewModel(User user)
        {
            _User = user;
        }
    }
}
