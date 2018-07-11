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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kinect2FaceBasics_NET;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System.ComponentModel;
using System.Globalization;

namespace Face_Recognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Sensor objek
        KinectSensor _sensor = null;

        // Frame warna, menampilkan RGB Stream
        ColorFrameReader _colorReader = null;

        // body frame reader, identifikasi bagian tubuh
        BodyFrameReader _bodyReader = null;

        // List bagian tubuh yg teridentifikasi sensor
        IList<Body> _bodies = null;

        // Frame wajah
        FaceFrameSource _faceSource = null;

        // Pembaca frame wajah
        FaceFrameReader _faceReader = null;

        private string statusText;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            // Inisialisasi Kinect 

            _sensor = KinectSensor.GetDefault();
            if (_sensor == null)
            {
                statusText = "Kinect Tidak Ada";
            }

            if (_sensor != null)
            {
                _sensor.Open();

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;
                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                // Inisialisasi sumber wajah dengan fitur

                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace
                                                             | FaceFrameFeatures.FaceEngagement
                                                             | FaceFrameFeatures.Glasses
                                                             | FaceFrameFeatures.Happy
                                                             | FaceFrameFeatures.LeftEyeClosed
                                                             | FaceFrameFeatures.MouthOpen
                                                             | FaceFrameFeatures.PointsInColorSpace
                                                             | FaceFrameFeatures.RightEyeClosed);
                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;

                FrameDescription frameDescription = _sensor.ColorFrameSource.FrameDescription;
            }
        }
        //BodyReader Frame Arrived
        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);
                    Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }
        // Color reader
        void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }
        }
        //Face Reader Frame Arrived
        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get face frame result
                    FaceFrameResult result = frame.FaceFrameResult;

                    if (result != null)
                    {
                        // Get face point, mapped in color space
                        var mataKiri = result.FacePointsInColorSpace[FacePointType.EyeLeft];
                        var mataKanan = result.FacePointsInColorSpace[FacePointType.EyeRight];
                        var hidung = result.FacePointsInColorSpace[FacePointType.Nose];
                        var mulutBagianKiri = result.FacePointsInColorSpace[FacePointType.MouthCornerLeft];
                        var mulutBagianKanan = result.FacePointsInColorSpace[FacePointType.MouthCornerRight];

                        // Get the characteristics
                        var mataKiriTertutup = result.FaceProperties[FaceProperty.LeftEyeClosed];
                        var mataKananTertutup = result.FaceProperties[FaceProperty.RightEyeClosed];
                        var mulutTerbuka = result.FaceProperties[FaceProperty.MouthOpen];
                        var senyum = result.FaceProperties[FaceProperty.Happy];
                        var memakaiKacamata = result.FaceProperties[FaceProperty.WearingGlasses];

                        // Position the canvas UI elements
                        Canvas.SetLeft(ellipseMataKiri, mataKiri.X - ellipseMataKiri.Width / 2.0);
                        Canvas.SetTop(ellipseMataKiri, mataKiri.Y - ellipseMataKiri.Height / 2.0);

                        Canvas.SetLeft(ellipseMataKanan, mataKanan.X - ellipseMataKanan.Width / 2.0);
                        Canvas.SetTop(ellipseMataKanan, mataKanan.Y - ellipseMataKanan.Height / 2.0);

                        Canvas.SetLeft(ellipseHidung, hidung.X - ellipseHidung.Width / 2.0);
                        Canvas.SetTop(ellipseHidung, hidung.Y - ellipseHidung.Height / 2.0);

                        Canvas.SetLeft(ellipseMulut, ((mulutBagianKanan.X + mulutBagianKiri.X) / 2.0) - ellipseMulut.Width / 2.0);
                        Canvas.SetTop(ellipseMulut, ((mulutBagianKanan.Y + mulutBagianKiri.Y) / 2.0) - ellipseMulut.Height / 2.0);
                        ellipseMulut.Width = Math.Abs(mulutBagianKanan.X - mulutBagianKiri.X);


                        // Display or hide the ellipses
                        if (mataKiriTertutup == DetectionResult.Yes || mataKiriTertutup == DetectionResult.Maybe)
                        {
                            ellipseMataKiri.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ellipseMataKiri.Visibility = Visibility.Visible;
                        }

                        if (mataKananTertutup == DetectionResult.Yes || mataKananTertutup == DetectionResult.Maybe)
                        {
                            ellipseMataKanan.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ellipseMataKanan.Visibility = Visibility.Visible;
                        }

                        if (mulutTerbuka == DetectionResult.Yes || mulutTerbuka == DetectionResult.Maybe)
                        {
                            ellipseMulut.Height = 50.0;
                        }
                        else
                        {
                            ellipseMulut.Height = 20.0;
                        }
                    }
                }
            }
        }
        //Window Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_faceReader != null)
            {
                _faceReader.Dispose();
                _faceReader = null;
            }
            if (_faceSource != null)
            {
                _faceSource.Dispose();
                _faceSource = null;
            }
            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
                _bodyReader = null;
            }
            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }
        //Error Prevention
        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Status Text"));
                    }
                }
            }
        }
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (_sensor != null)
            {
                // on failure, set the status text
                this.StatusText = _sensor.IsAvailable ? "Kinect is Connected"
                                                                : "Kinect is Not Connected";
            }
        }
    }
}
