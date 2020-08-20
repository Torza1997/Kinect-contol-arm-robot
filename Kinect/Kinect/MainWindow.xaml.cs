using Microsoft.Kinect;
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



namespace Kinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        string Message;
        KinectSensor mikinect;


        
        System.IO.Ports.SerialPort SP = new System.IO.Ports.SerialPort();
        System.IO.Ports.SerialPort SP2 = new System.IO.Ports.SerialPort();
        //"COM3",9600,System.IO.Ports.Parity.None,8,System.IO.Ports.StopBits.One

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //startKinect();
        }
        private void Window_Closing(object sender,System.ComponentModel.CancelEventArgs e){
           mikinect.Stop();
           SP.Close();
           SP2.Close();
        }
    
        void startKinect()
        {
            Connect_arduino();
            // Check to see if a Kinect is available
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinects detected", "Camera Viewer");
                Application.Current.Shutdown();
                return;
            }
            // Start the Kinect and enable both video and skeleton streams
            try
            {
                // Get the first Kinect on the computer
                mikinect = KinectSensor.KinectSensors[0];
                mikinect.ColorStream.Enable();
                mikinect.SkeletonStream.Enable(new TransformSmoothParameters
                {
                    Smoothing = 0.1f,
                    Correction = 0.2f,
                    Prediction = 0.2f,
                    JitterRadius = 0.0f,
                    MaxDeviationRadius = 0.2f
                }
                   );
                mikinect.Start();
            }
            catch
            {
                MessageBox.Show("Kinect initialise failed", "Camera Viewer");
                Application.Current.Shutdown();
                return;
            }
            //conectport arduino

            // connect a handler to the event that fires when new frames are available
            mikinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(myKinect_ColorFrameReady);
            mikinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

        }

        byte[] colorData = null;
        WriteableBitmap colorImageBitmap = null;

        private void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
           
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) return;

                if (colorData == null)
                    colorData = new byte[colorFrame.PixelDataLength];

                colorFrame.CopyPixelDataTo(colorData);

                if (colorImageBitmap == null)
                {
                    colorImageBitmap = new WriteableBitmap(
                        colorFrame.Width,
                        colorFrame.Height,
                        96,  // DpiX
                        96,  // DpiY
                        PixelFormats.Bgr32,
                        null);
                }

                colorImageBitmap.WritePixels(
                    new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                    colorData, // video data
                    colorFrame.Width * colorFrame.BytesPerPixel, // stride,
                    0   // offset into the array - start at 0
                    );

                kinectVideo.Source = colorImageBitmap;
            }
        }

        Brush brushRed = new SolidColorBrush(Colors.Red);
        Brush brushBlue = new SolidColorBrush(Colors.Blue);
       

        private void drawLine(Joint j1, Joint j2)
        {
            
            //if any joint is not tracked
            if (j1.TrackingState == JointTrackingState.NotTracked || j2.TrackingState == JointTrackingState.NotTracked)
            {
                //do not draw
                return;
            }


            //line properties

            Line bone = new Line();

            //line thickness            
            bone.StrokeThickness = 5;

            //if any joint is inferred
            
            if (j1.TrackingState == JointTrackingState.Inferred || j2.TrackingState == JointTrackingState.Inferred)
            {
                //set color to red
                
                bone.Stroke = brushRed;
            }
            else if (j1.TrackingState == JointTrackingState.Tracked && j2.TrackingState == JointTrackingState.Tracked)
            {
                //both joints are tracked, set color to green
                bone.Stroke = brushBlue;
            }

            //map joint position to display location
            //starting point
            ColorImagePoint j1p = mikinect.CoordinateMapper.MapSkeletonPointToColorPoint(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            bone.X1 = j1p.X;
            bone.Y1 = j1p.Y;

            //ending point
            ColorImagePoint j2p = mikinect.CoordinateMapper.MapSkeletonPointToColorPoint(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            bone.X2 = j2p.X;
            bone.Y2 = j2p.Y;

            //add line to canvas
            skeletonCanvas.Children.Add(bone);
        }
       
            private void CameraPosition(ColorImagePoint  joint) {
            Ellipse ellipse = new Ellipse
            {
              Width =70,
              Height = 70,
              Fill = new SolidColorBrush(Colors.LightGreen)
            };

            Canvas.SetLeft(ellipse, joint.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Y - ellipse.Height / 2);

           skeletonCanvas.Children.Add(ellipse);

        }



        int personID;

        private void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            
            // Remove the old skeleton (if exists)
            skeletonCanvas.Children.Clear();
            Skeleton[] skeletons = null;

            //copy skeleton data to skeleton array
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                Message = "No Skeleton Data";
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
             
                }
                
            }
            
            //if no skeleton data
            if (skeletons == null) {
                return;

            }

            if (personID == 0)
            {
                //textID.Text = "Skeleton size = " + skeletons.Length;
                //find ID of the first tracked skeleton
                foreach (Skeleton skeleton in skeletons)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        personID = skeleton.TrackingId;
                        //allow track on specific skeleton (not all)
                        mikinect.SkeletonStream.AppChoosesSkeletons = true;
                        //track only this skeleton
                        mikinect.SkeletonStream.ChooseSkeletons(personID);
                        //neglect other skeletonsD;
                        TextBox2.Text = "Skl_ID"+ skeleton.TrackingId;
                        break;
                    }
                }
            }
            else {
                //for each skeleton
          
                foreach (Skeleton skeleton in skeletons)
                {
                    //if skeleton is tracked

                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Joint handRight = skeleton.Joints[JointType.HandRight];
                        Joint handLeft = skeleton.Joints[JointType.HandLeft];

                        //Joint shoulderRight = skeleton.Joints[JointType.ShoulderRight];
                        //Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                        Joint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];

                        ColorImagePoint handRightPS = mikinect.CoordinateMapper.MapSkeletonPointToColorPoint(handRight.Position, ColorImageFormat.RgbResolution640x480Fps30);
                        ColorImagePoint handLeftPS = mikinect.CoordinateMapper.MapSkeletonPointToColorPoint(handLeft.Position, ColorImageFormat.RgbResolution640x480Fps30);
                        //ColorImagePoint headerPS = mikinect.CoordinateMapper.MapSkeletonPointToColorPoint(header.Position, ColorImageFormat.RgbResolution640x480Fps30);

                        //CameraPosition(head, headerPS);

                        if (handRight.TrackingState == JointTrackingState.Inferred || handLeft.TrackingState == JointTrackingState.Inferred)
                        {
                           
                        }
                        else
                        {

                            SkeletonPoint handRightP = handRight.Position;
                            SkeletonPoint handLeftP = handLeft.Position;

                             
                            //SkeletonPoint shoulderRightP = shoulderRight.Position;
                            //SkeletonPoint shoulderLeftP = shoulderLeft.Position;
                            SkeletonPoint shoulderCenterP = shoulderCenter.Position;

                            
                           
                            CameraPosition(handRightPS);
                            CameraPosition(handLeftPS);
                            
                            Handleft(handLeftP);
                            HandRight(handRightP);
                            
                            ShowText(handRightPS, handLeftPS, handRightP, handLeftP, shoulderCenterP);

                        }
                    }
                }
                TextBoxs.Text = Message;
            }
        }

        float HLPX, HLPY , HLPZ;
        float HRPX, HRPY, HRPZ;
        float SDCX, SDCY, SDCZ;
        float count;

        void Handleft(SkeletonPoint handLeftP) {

            HLPX = handLeftP.X *100; HLPY = handLeftP.Y * 100; HLPZ = handLeftP.Z*100;
            //SDCX = shoulderCenterP.X * 100; SDCY = shoulderCenterP.Y * 100; SDCZ = shoulderCenterP.Z * 100;
            //count = HLPX - SDCX;
            String L = "L" + HLPY.ToString() + "," + HLPZ.ToString()+",";
            String S = "S"+ HLPX.ToString() + "," ;
            SP2.Write(S);
            SP.Write(L);
            //SP.Write(HLPX.ToString()+",");


        }

        void HandRight(SkeletonPoint handRightP) {
            HRPX = handRightP.X * 100; HRPY = handRightP.Y * 100; HRPZ = handRightP.Z * 100;
            //SDCX = shoulderCenterP.X * 100; SDCY = shoulderCenterP.Y * 100; SDCZ = shoulderCenterP.Z * 100;
            String R = "R" + HRPX.ToString() + "," + HRPY.ToString() + ","+ HRPZ.ToString()+",";
            SP.Write(R);

        }
        private void ShowText(ColorImagePoint handLeftPS, ColorImagePoint handRightPS, SkeletonPoint handRightP, SkeletonPoint handLeftP, SkeletonPoint shoulderCenterP) {
            Message = string.Format("handleft2: X:{0:0.0} Y:{1:0.0}\n Hand Righ: X:{2:0.0} Y:{3:0.0} Z:{4:0.0} \n Hand Left: X:{5:0.0} Y:{6:0.0} Z:{7:0.0} \n handright2: X:{8:0.0} Y:{9:0.0}",
                           
                            handLeftPS.X,
                            handLeftPS.Y,

                            (handRightP.X * 100) /*- (shoulderCenterP.X * 100)*/,
                            handRightP.Y * 100,
                            handRightP.Z * 100,

                            handLeftP.X * 100,
                            handLeftP.Y * 100,
                            handLeftP.Z * 100,

                            handRightPS.X,
                            handRightPS.Y

                            );
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            startKinect();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            mikinect.Stop();
            SP.Close();
            SP2.Close();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            mikinect.SkeletonStream.AppChoosesSkeletons = false;
            personID = 0;
        }
        void Connect_arduino(){
            try
            {
                SP.PortName = "COM4"; //port arduino
                SP.BaudRate = 115200;
                SP.WriteTimeout = 250;
                SP.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Port Error COM4");
            }

            try
            {
                SP2.PortName = "COM3"; //port arduino
                SP2.BaudRate = 9600;
                SP2.WriteTimeout = 500;
                SP2.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Port Error COM3");
            }
            
        }
    }
}

namespace Text {
    public class savecode{
        /*
        //draw head to neck
        drawLine(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
        //draw neck to spine
        drawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);
        //draw spine to hip center
        drawLine(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);

        //draw left arm
        drawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
        drawLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
        drawLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
        drawLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

        //draw right arm
        drawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
        drawLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
        drawLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
        drawLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

        //draw left leg
        drawLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
        drawLine(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
        drawLine(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
        drawLine(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

        //draw right leg
        drawLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
        drawLine(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
        drawLine(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
        drawLine(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);
        */
    }
} 
