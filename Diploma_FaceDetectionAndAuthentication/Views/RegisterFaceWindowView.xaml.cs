using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Emgu.CV.CvEnum;
using System.IO;
using System.Drawing;
using Diploma_FaceDetectionAndAuthentication.ViewModels;
using Diploma_FaceDetectionAndAuthentication.Services;

namespace Diploma_FaceDetectionAndAuthentication.Views
{
    /// <summary>
    /// Interaction logic for RegisterFaceWindowView.xaml
    /// </summary>
    public partial class RegisterFaceWindowView : Window
    {
        public FacialAuth FAuth;
        private readonly RegisterFaceViewModel _ViewModel;

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            FAuth = new FacialAuth(this.txtTip, this.btnStart, false);

            this.grid1.Children.Add(FAuth.Host);
        }

        public RegisterFaceWindowView(RegisterFaceViewModel vm)
        {
            InitializeComponent();
            _ViewModel = vm;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            FAuth.IsStarted = true;
            if (FAuth.RegisterFace(_ViewModel._User))
            {
                MessageBox.Show("Your face registered successfully");

                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
