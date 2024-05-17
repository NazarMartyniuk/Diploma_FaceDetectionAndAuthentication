using Diploma_FaceDetectionAndAuthentication.Models;
using Diploma_FaceDetectionAndAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Diploma_FaceDetectionAndAuthentication.ViewModels
{
    public abstract class AuthenticationViewModel : NotifyPropertyChanged
    {
        protected readonly ProjectManager _ProjectManager;

        private string _Email = "";
        public string Email
        {
            get { return this.GetProperty(_Email); }
            set { this.SetProperty(ref _Email, value); }
        }

        private string _Password = "";
        public string Password
        {
            get { return this.GetProperty(_Password); }
            set { this.SetProperty(ref _Password, value); }
        }

        private ICommand _BackButtonCommand;
        public ICommand BackButtonCommand
        {
            get { return this.GetProperty(_BackButtonCommand); }
            set { this.SetProperty(ref _BackButtonCommand, value); }
        }

        public AuthenticationViewModel(ProjectManager manager)
        {
            _ProjectManager = manager;
            BackButtonCommand = new BackCommand(_ProjectManager);
        }

        private class BackCommand : ICommand
        {
            private readonly ProjectManager _Manager;
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _Manager.SelectedViewModel = null;
            }

            public BackCommand(ProjectManager manager)
            {
                _Manager = manager;
            }
        }
    }
}
