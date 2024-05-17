using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Windows.Forms.Integration;
using System.IO;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using Diploma_FaceDetectionAndAuthentication.Models;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public class FacialAuth : IDisposable
    {
        public WindowsFormsHost Host { get; set; }
        public bool IsStarted { get; set; }

        private DispatcherTimer _Timer;
        private Image<Bgr, Byte> _CurrentFrame;
        private Capture _Grabber;
        private HaarCascade _Face;
        private Image<Gray, byte> _Result;
        private Image<Gray, byte> _Gray = null;
        private List<Image<Gray, byte>> _CurrentImages = new List<Image<Gray, byte>>();
        Image<Gray, byte> _TrainedFace = null;
        private List<string> _NamePersons = new List<string>();
        private bool _IsAuth;

        private ImageBox _ImgBox;
        private TextBlock _TxtTip;
        private System.Windows.Controls.Button _StartButton = null;

        public FacialAuth(TextBlock txtTip = null, System.Windows.Controls.Button startButton = null, bool isAuth = true)
        {
            _TxtTip = txtTip;
            _StartButton = startButton;
            _IsAuth = isAuth;

            _Face = new HaarCascade(System.AppDomain.CurrentDomain.BaseDirectory + "/haarcascade_frontalface_default.xml");

            Host = new WindowsFormsHost();
            Host.Width = 320;
            Host.Height = 240;

            _ImgBox = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)_ImgBox).BeginInit();

            _ImgBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _ImgBox.Name = "imageBoxFrameGrabber";
            _ImgBox.Size = new System.Drawing.Size(320, 240);
            _ImgBox.TabIndex = 4;
            _ImgBox.TabStop = false;

            Host.Child = _ImgBox;

            _Grabber = new Capture();
            _Grabber.QueryFrame();

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromMilliseconds(1);
            _Timer.Tick += new EventHandler(FrameGrabber);
            _Timer.Start();
        }

        private void FrameGrabber(object sender, EventArgs e)
        {
            _NamePersons.Add("");

            _CurrentFrame = _Grabber.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);

            _Gray = _CurrentFrame.Convert<Gray, Byte>();

            MCvAvgComp[][] facesDetected = _Gray.DetectHaarCascade(
                  _Face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                  new System.Drawing.Size(20, 20));

            if (facesDetected[0].Length > 1 && !_IsAuth && !IsStarted)
            {
                _TxtTip.Text = "Cannot be more than one face in frame";
                _StartButton.IsEnabled = false;
            }
            else if (facesDetected[0].Length == 1)
            {
                if (_TxtTip != null && !IsStarted)
                {
                    _TxtTip.Text = "Watch straight to the camera. Move your head";
                    _StartButton.IsEnabled = true;
                }

                _Result = _CurrentFrame.Copy(facesDetected[0][0].rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                _CurrentFrame.Draw(facesDetected[0][0].rect, new Bgr(Color.Green), 2);
            }
            else if (!_IsAuth && !IsStarted)
            {
                _TxtTip.Text = "Try to move your face closer or turn on the light";
                _StartButton.IsEnabled = false;
            }

            _ImgBox.Image = _CurrentFrame;
        }

        public bool RegisterFace(User user)
        {
            _CurrentImages.Clear();

            int i = 0;
            while(i < 100)
            {
                FrameGrabber(this, null);

                _Gray = _Grabber.QueryGrayFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);

                MCvAvgComp[][] facesDetected = _Gray.DetectHaarCascade(
                        _Face,
                        1.2,
                        10,
                        Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                        new Size(20, 20));

                if (facesDetected[0].Length != 1)
                {
                    break;
                }

                _TrainedFace = _CurrentFrame.Copy(facesDetected[0][0].rect).Convert<Gray, byte>();
                _TrainedFace = _Result.Resize(100, 100, INTER.CV_INTER_CUBIC);

                _CurrentImages.Add(_TrainedFace);

                i++;
            }

            if (i == 100)
            {
                string userId = "";

                SqlConnection sqlConnectionUser = new SqlConnection(ConfigurationManager.ConnectionStrings["localDb"].ConnectionString);
                string userInsertQuery = "INSERT INTO [User](FirstName, LastName, BirthDate, Email, Password, PhoneNumber, City) VALUES(@firstName, @lastName, @birthDate, @email, @password, @phoneNumber, @city)";
                string getUserIdQuery = "SELECT [Id] FROM [User] WHERE [Email] = @email";

                SqlCommand sqlInsertUserCommand = new SqlCommand(userInsertQuery, sqlConnectionUser);
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@firstName", user.FirstName));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@lastName", user.LastName));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@birthDate", user.BirthDate.GetDateTimeFormats()[2]));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@email", user.Email));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@password", user.Password));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@phoneNumber", user.PhoneNumber));
                sqlInsertUserCommand.Parameters.Add(new SqlParameter("@city", user.City));

                SqlCommand sqlGetUserIdCommand = new SqlCommand(getUserIdQuery, sqlConnectionUser);
                sqlGetUserIdCommand.Parameters.Add(new SqlParameter("@email", user.Email));

                sqlConnectionUser.Open();
                sqlInsertUserCommand.ExecuteNonQuery();
                var reader = sqlGetUserIdCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    userId = reader["Id"].ToString();
                }

                sqlConnectionUser.Close();

                for (int j = 0; j < 100; j++)
                {
                    byte[] imageData = _CurrentImages[j].Bytes;

                    SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["localDb"].ConnectionString);
                    string qry = "INSERT INTO BiometricImage(UserId, Image) VALUES (@id, @image)";

                    SqlCommand sqlCommand = new SqlCommand(qry, sqlConnection);
                    sqlCommand.Parameters.Add(new SqlParameter("@id", userId));
                    sqlCommand.Parameters.Add(new SqlParameter("@image", imageData));

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }

                return true;
            }
            else
            {
                IsStarted = false;

                MessageBox.Show("Face registration is failed. Try again", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        public void Dispose()
        {
            _Timer.Stop();
            _Grabber = null;

            GC.Collect();
        }
    }
}
