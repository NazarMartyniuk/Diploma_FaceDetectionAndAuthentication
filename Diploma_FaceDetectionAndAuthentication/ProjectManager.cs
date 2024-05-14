using Diploma_FaceDetectionAndAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication
{
    public class ProjectManager : NotifyPropertyChanged
    {
        private bool _IsMainMenu = true;
        public bool IsMainMenu
        {
            get { return this.GetProperty(_IsMainMenu); }
            set { this.SetProperty(ref _IsMainMenu, value); }
        }
    }
}
