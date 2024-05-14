using Diploma_FaceDetectionAndAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diploma_FaceDetectionAndAuthentication.ViewModels
{
    public class LoginViewModel : AuthenticationViewModel
    {
        private ICommand _LoginButtonCommand;
        public ICommand LoginButtonCommand
        {
            get { return this.GetProperty(_LoginButtonCommand); }
            set { this.SetProperty(ref _LoginButtonCommand, value); }
        }

        public LoginViewModel(ProjectManager projectManager) : base(projectManager)
        {
            LoginButtonCommand = new LoginCredentialsCommand(this);
        }

        private class LoginCredentialsCommand : ICommand
        {
            private readonly LoginViewModel _ParentViewModel;
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter != null)
                {
                    var passBox = (PasswordBox)parameter;

                    string password = passBox.Password;
                    _ParentViewModel.Password = password;
                }
            }

            public LoginCredentialsCommand(LoginViewModel loginViewModel)
            {
                _ParentViewModel = loginViewModel;
            }
        }
    }
}
