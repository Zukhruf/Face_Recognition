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
        FaceFrameSource[] _faceSource = null;

        // Pembaca frame wajah
        FaceFrameReader[] _faceReader = null;

        // Hasil
        FaceFrameResult[] _faceResult = null;

        private List<Brush> _faceBrush;

        private string statusText;

        private int bodyCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            // Inisialisasi Kinect 
           
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;
                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                this.bodyCount = _sensor.BodyFrameSource.BodyCount;

                this._faceReader = new FaceFrameReader[this.bodyCount];
                this._faceSource = new FaceFrameSource[this.bodyCount];
                this._faceResult = new FaceFrameResult[this.bodyCount];
                // Inisialisasi sumber wajah dengan fitur

                for (int i = 0; i < this.bodyCount; i++)
                {
                    _faceSource[i] = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace
                                                             | FaceFrameFeatures.FaceEngagement
                                                             | FaceFrameFeatures.Glasses
                                                             | FaceFrameFeatures.Happy
                                                             | FaceFrameFeatures.LeftEyeClosed
                                                             | FaceFrameFeatures.MouthOpen
                                                             | FaceFrameFeatures.PointsInColorSpace
                                                             | FaceFrameFeatures.RightEyeClosed);
                    _faceReader[i] = _faceSource[i].OpenReader();
                    _faceReader[i].FrameArrived += FaceReader_FrameArrived;
                }
            }

            this._faceBrush = new List<Brush>()
                {
                    Brushes.Blue,
                    Brushes.Brown,
                    Brushes.Green,
                    Brushes.OrangeRed,
                    Brushes.Black
                };

            this._sensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
        }

        public int Bodycount
        {
            set
            {
                this.bodyCount = _sensor.BodyFrameSource.BodyCount;
            }
            get
            {
                return this.bodyCount;
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
           
                    for (int i = 0; i < this.bodyCount; i++)
                    {
                        if (_faceSource[i].IsTrackingIdValid)
                        {
                            if (body != null)
                            {
                                _faceSource[i].TrackingId = body.TrackingId;
                            }
                        }
                    }
                }
            }
        }

        //private void DrawFaceFrameResult(int faceIndex, FaceFrameResult faceResult, DrawingContext dc)
        //{
        //    Brush drawingBrush = this._faceBrush[0];
        //    if (faceIndex < this.bodyCount)
        //    {
        //        drawingBrush = this._faceBrush[faceIndex];
        //    }

        //    Pen pen = new Pen(drawingBrush, 20);

        //    var faceBoxSource = _faceResult.FaceBoundingBoxInColorSpace;
        //    Rect faceBox = new Rect(faceBoxSource.Left, faceBoxSource.Top, faceBoxSource.Right - faceBoxSource.Left, faceBoxSource.Top - faceBoxSource.Bottom);
        //    dc.DrawRectangle(null, pen, faceBox);

        //    if (faceResult.FacePointsInColorSpace != null)
        //    {
        //        foreach (PointF point in faceResult.FacePointsInColorSpace.Values)
        //        {
        //            dc.DrawEllipse(null, pen, new Point(point.X, point.Y), 1.0, 1.0);
        //        }
        //    }

        //    string faceText = string.Empty;
        //    if (faceResult.FaceProperties != null)
        //    {
        //        foreach (var fp in faceResult.FaceProperties)
        //        {
        //            faceText += fp.Key.ToString() + " : ";

        //            if (fp.Value == DetectionResult.Maybe)
        //            {
        //                faceText += DetectionResult.No + "\n";
        //            }
        //            else
        //            {
        //                faceText += fp.Key.ToString() + "\n";
        //            }
        //        }
        //    }
        //    dc.DrawText(new FormattedText(
        //        faceText,
        //        CultureInfo.GetCultureInfo("en-us"),
        //        FlowDirection.LeftToRight,
        //        new Typeface("Georgia"),
        //        30, drawingBrush),
        //        faceText);
        //}

        // Color reader

        // Color Frame
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
                    for (int i = 0; i < this.bodyCount; i++)
                    {
                        this._faceResult[i] = frame.FaceFrameResult;
                        // get face frame result
                        if (_faceResult[i] != null)
                        {
                            // Get face point, mapped in color space
                            var mataKiri = _faceResult[i].FacePointsInColorSpace[FacePointType.EyeLeft];
                            var mataKanan = _faceResult[i].FacePointsInColorSpace[FacePointType.EyeRight];
                            var hidung = _faceResult[i].FacePointsInColorSpace[FacePointType.Nose];
                            var mulutBagianKiri = _faceResult[i].FacePointsInColorSpace[FacePointType.MouthCornerLeft];
                            var mulutBagianKanan = _faceResult[i].FacePointsInColorSpace[FacePointType.MouthCornerRight];

                            // Get the characteristics
                            var mataKiriTertutup = _faceResult[i].FaceProperties[FaceProperty.LeftEyeClosed];
                            var mataKananTertutup = _faceResult[i].FaceProperties[FaceProperty.RightEyeClosed];
                            var mulutTerbuka = _faceResult[i].FaceProperties[FaceProperty.MouthOpen];
                            var senyum = _faceResult[i].FaceProperties[FaceProperty.Happy];
                            var memakaiKacamata = _faceResult[i].FaceProperties[FaceProperty.WearingGlasses];

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
        }
        //Window Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < this.bodyCount; i++)
            {
                if (_faceReader[i] != null)
                {
                    _faceReader[i].Dispose();
                    _faceReader[i] = null;
                }
                if (_faceSource[i] != null)
                {
                    _faceSource[i].Dispose();
                    _faceSource[i] = null;
                }
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
                this.StatusText = _sensor.IsAvailable ? "Kinect is Connected" : "Kinect is Not Connected";
            }
        }
    }
}
