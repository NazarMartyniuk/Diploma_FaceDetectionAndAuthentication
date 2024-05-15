using Diploma_FaceDetectionAndAuthentication.Services;
using Diploma_FaceDetectionAndAuthentication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        private AuthenticationViewModel _SelectedViewModel;
        public AuthenticationViewModel SelectedViewModel
        {
            get { return this.GetProperty(_SelectedViewModel); }
            set
            {
                IsMainMenu = value == null;

                this.SetProperty(ref _SelectedViewModel, value);
            }
        }

        private RegisterFaceViewModel _RegisterFaceViewModel = null;
        public RegisterFaceViewModel RegisterFaceViewModel
        {
            get { return this.GetProperty(_RegisterFaceViewModel); }
            set
            {
                SelectedViewModel = null;
                IsMainMenu = value == null;

                this.SetProperty(ref _RegisterFaceViewModel, value);
            }
        }

        private ICommand _LoginButtonCommand;
        public ICommand LoginButtonCommand
        {
            get { return this.GetProperty(_LoginButtonCommand); }
            set { this.SetProperty(ref _LoginButtonCommand, value); }
        }

        private ICommand _RegisterButtonCommand;
        public ICommand RegisterButtonCommand
        {
            get { return this.GetProperty(_RegisterButtonCommand); }
            set { this.SetProperty(ref _RegisterButtonCommand, value); }
        }

        public ProjectManager()
        {
            LoginButtonCommand = new LoginCommand(this);
            RegisterButtonCommand = new LoginCommand(this, false);
        }

        private class LoginCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private readonly ProjectManager _Manager;
            private bool _IsLogin;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (_IsLogin)
                {
                    _Manager.SelectedViewModel = new LoginViewModel(_Manager);
                }
                else
                {
                    _Manager.SelectedViewModel = new RegisterViewModel(_Manager);
                }
            }

            public LoginCommand(ProjectManager manager, bool isLogin = true)
            {
                _Manager = manager;
                _IsLogin = isLogin;
            }
        }
    }
}
