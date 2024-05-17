using Diploma_FaceDetectionAndAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Diploma_FaceDetectionAndAuthentication.Views
{
    /// <summary>
    /// Interaction logic for LoginFaceWindowView.xaml
    /// </summary>
    public partial class LoginFaceWindowView : Window
    {
        public FacialAuth FAuth;

        private DispatcherTimer _Timer;

        public LoginFaceWindowView(int id)
        {
            InitializeComponent();
            FAuth = new FacialAuth(id: id);

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromMilliseconds(10);
            _Timer.Tick += CheckIsAuthenticated;
            _Timer.Start();
        }

        private void CheckIsAuthenticated(object sender, EventArgs e)
        {
            if (FAuth.IsAuthenticated)
            {
                _Timer.Stop();
                MessageBox.Show("You have been successfully authenticated!");

                this.DialogResult = true;
                this.Close();
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.grid1.Children.Add(FAuth.Host);
        }
    }
}
