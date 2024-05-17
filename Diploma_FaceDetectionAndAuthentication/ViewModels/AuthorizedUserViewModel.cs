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
    public class AuthorizedUserViewModel : NotifyPropertyChanged
    {
        private string _FirstName = "";
        public string FirstName
        {
            get { return this.GetProperty(_FirstName); }
            set { this.SetProperty(ref _FirstName, value); }
        }

        private string _LastName = "";
        public string LastName
        {
            get { return this.GetProperty(_LastName); }
            set { this.SetProperty(ref _LastName, value); }
        }

        private string _Email = "";
        public string Email
        {
            get { return this.GetProperty(_Email); }
            set { this.SetProperty(ref _Email, value); }
        }

        private DateTime _BirthDate = DateTime.Today;
        public DateTime BirthDate
        {
            get { return this.GetProperty(_BirthDate); }
            set 
            {
                this.SetProperty(ref _BirthDate, value);
            }
        }

        public string PrintableBirthDate
        {
            get
            {
                using (this.Subscribe())
                {
                    return BirthDate.ToShortDateString();
                }
            }
        }

        private string _PhoneNumber = "";
        public string PhoneNumber
        {
            get { return this.GetProperty(_PhoneNumber); }
            set { this.SetProperty(ref _PhoneNumber, value); }
        }

        private string _City = "";
        public string City
        {
            get { return this.GetProperty(_City); }
            set { this.SetProperty(ref _City, value); }
        }

        public AuthorizedUserViewModel(User user, ProjectManager projectManager)
        {
            BackButtonCommand = new BackCommand(projectManager);

            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            BirthDate = user.BirthDate;
            PhoneNumber = user.PhoneNumber;
            City = user.City;
        }

        private ICommand _BackButtonCommand;
        public ICommand BackButtonCommand
        {
            get { return this.GetProperty(_BackButtonCommand); }
            set { this.SetProperty(ref _BackButtonCommand, value); }
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
                _Manager.AuthorizedUserViewModel = null;
            }

            public BackCommand(ProjectManager manager)
            {
                _Manager = manager;
            }
        }
    }
}
