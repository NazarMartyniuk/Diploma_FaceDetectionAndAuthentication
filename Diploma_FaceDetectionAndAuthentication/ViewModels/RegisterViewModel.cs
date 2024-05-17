using Diploma_FaceDetectionAndAuthentication.Models;
using Diploma_FaceDetectionAndAuthentication.Services;
using Diploma_FaceDetectionAndAuthentication.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Diploma_FaceDetectionAndAuthentication.ViewModels
{
    public class RegisterViewModel : AuthenticationViewModel
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

        private DateTime _BirthDate = DateTime.Today;
        public DateTime BirthDate
        {
            get { return this.GetProperty(_BirthDate); }
            set { this.SetProperty(ref _BirthDate, value); }
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

        private void Register()
        {
            {
                string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
                Match isMatch = Regex.Match(Email, pattern, RegexOptions.IgnoreCase);

                if (!isMatch.Success)
                {
                    MessageBox.Show("Wrong email format", "Email error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["localDb"].ConnectionString))
                {
                    string query = "SELECT * FROM [User] WHERE Email = @email";
                    SqlCommand cmd = new SqlCommand(query, db);
                    cmd.Parameters.AddWithValue("@email", Email);
                    db.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("User with this email already registered.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
            }

            {
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");

                var isValidated = hasNumber.IsMatch(Password) && hasUpperChar.IsMatch(Password) && hasMinimum8Chars.IsMatch(Password);

                if (!isValidated)
                {
                    MessageBox.Show("Wrong password format. Password must contain at least a number, one uppercase letter and 8 characters long", "Password error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            {
                if (string.IsNullOrEmpty(FirstName.Trim()))
                {
                    MessageBox.Show("First name cannot be empty", "First name error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            {
                if (string.IsNullOrEmpty(LastName.Trim()))
                {
                    MessageBox.Show("Last name cannot be empty", "Last name error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            {
                if (string.IsNullOrEmpty(PhoneNumber.Trim()))
                {
                    MessageBox.Show("Phone number cannot be empty", "Phone number error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (PhoneNumber.Any(c => !char.IsDigit(c)))
                {
                    MessageBox.Show("Phone number cannot contain letter or symbol", "Phone number error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            {
                if (string.IsNullOrEmpty(City.Trim()))
                {
                    MessageBox.Show("City cannot be empty", "City error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(Password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string passwordHash = Convert.ToBase64String(hashBytes);

            User user = new User()
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = passwordHash,
                BirthDate = BirthDate,
                PhoneNumber = PhoneNumber,
                City = City
            };

            RegisterFaceViewModel viewModel = new RegisterFaceViewModel(user);
            var authWindow = new RegisterFaceWindowView(viewModel);

            if (authWindow.ShowDialog() == true)
            {
                this._ProjectManager.SelectedViewModel = null;
            }
            authWindow.FAuth.Dispose();
        }

        public RegisterViewModel(ProjectManager projectManager) : base(projectManager)
        {
            RegisterButtonCommand = new RegisterCredentialsCommand(this);
        }

        private ICommand _RegisterButtonCommand;
        public ICommand RegisterButtonCommand
        {
            get { return this.GetProperty(_RegisterButtonCommand); }
            set { this.SetProperty(ref _RegisterButtonCommand, value); }
        }

        private class RegisterCredentialsCommand : ICommand
        {
            private readonly RegisterViewModel _ParentViewModel;
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

                    _ParentViewModel.Register();
                }
            }

            public RegisterCredentialsCommand(RegisterViewModel registerViewModel)
            {
                _ParentViewModel = registerViewModel;
            }
        }
    }
}
