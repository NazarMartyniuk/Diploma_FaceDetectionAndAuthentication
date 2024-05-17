using Diploma_FaceDetectionAndAuthentication.Models;
using Diploma_FaceDetectionAndAuthentication.Services;
using Diploma_FaceDetectionAndAuthentication.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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

        private void Login()
        {
            if (string.IsNullOrEmpty(Email))
            {
                MessageBox.Show("Email cannot be empty.", "Empty email", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("No such user with this email.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    while (reader.Read())
                    {
                        string password = reader["Password"].ToString();

                        byte[] hashBytes = Convert.FromBase64String(password);
                        byte[] salt = new byte[16];
                        Array.Copy(hashBytes, 0, salt, 0, 16);

                        var pbkdf2 = new Rfc2898DeriveBytes(Password, salt, 100000);
                        byte[] hash = pbkdf2.GetBytes(20);

                        for (int i = 0; i < 20; i++)
                        {
                            if (hashBytes[i + 16] != hash[i])
                            {
                                MessageBox.Show("Invalid password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                db.Close();
                                return;
                            }
                        }

                        User user = new User()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            BirthDate = DateTime.Parse(reader["BirthDate"].ToString()),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            City = reader["City"].ToString(),
                        };

                        var auth = new LoginFaceWindowView(user.Id);

                        if (auth.ShowDialog() == true)
                        {
                            this._ProjectManager.AuthorizedUserViewModel = new AuthorizedUserViewModel(user, _ProjectManager);
                        }
                        auth.FAuth.Dispose();
                    }

                    db.Close(); 
                }
            }
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

                    _ParentViewModel.Login();
                }
            }

            public LoginCredentialsCommand(LoginViewModel loginViewModel)
            {
                _ParentViewModel = loginViewModel;
            }
        }
    }
}
