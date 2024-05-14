using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Diploma_FaceDetectionAndAuthentication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;
        WindowsFormsHost host;
        Emgu.CV.UI.ImageBox imgBox;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            host = new WindowsFormsHost();
            host.Width = 320;
            host.Height = 240;

            imgBox = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)imgBox).BeginInit();

            imgBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            imgBox.Location = new System.Drawing.Point(12, 12);
            imgBox.Name = "imageBoxFrameGrabber";
            imgBox.Size = new System.Drawing.Size(320, 240);
            imgBox.TabIndex = 4;
            imgBox.TabStop = false;

            host.Child = imgBox;
            this.grid1.Children.Add(host);

            grabber = new Capture();
            grabber.QueryFrame();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(40);
            timer.Tick += new EventHandler(FrameGrabber);
            timer.Start();
        }

        

        public MainWindow()
        {
            InitializeComponent();
            face = new HaarCascade(System.AppDomain.CurrentDomain.BaseDirectory + "/haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinfo = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show("Nothing in binary database, please add face.", "Trained faces load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            NamePersons.Add("");

            currentFrame = grabber.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);

            gray = currentFrame.Convert<Gray, Byte>();

            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.SCALE_IMAGE,
                  new System.Drawing.Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                currentFrame.Draw(f.rect, new Bgr(Color.Green), 2);


                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    name = recognizer.Recognize(result);

                    currentFrame.Draw(name, ref font, new System.Drawing.Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");

            }

            imgBox.Image = currentFrame;
        }
    }
}
