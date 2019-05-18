﻿using System;
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
using Microsoft.Kinect;
using System.IO;

namespace Skeletondata
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        private readonly Brush[] skeletonBrushes;//绘图笔刷
        private Skeleton[] frameSkeletons;

        public MainWindow()
        {
            InitializeComponent();
            skeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

        }

        public KinectSensor KinectDevice
        {
            get { return this.kinectDevice; }
            set
            {
                if (this.kinectDevice != value)
                {
                    //Uninitialize
                    if (this.kinectDevice != null)
                    {
                        this.kinectDevice.Stop();
                        this.kinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this.kinectDevice.SkeletonStream.Disable();
                        this.frameSkeletons = null;
                    }

                    this.kinectDevice = value;

                    //Initialize
                    if (this.kinectDevice != null)
                    {
                        if (this.kinectDevice.Status == KinectStatus.Connected)
                        {
                            this.kinectDevice.SkeletonStream.Enable();
                            this.frameSkeletons = new Skeleton[this.kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
                            this.kinectDevice.Start();
                        }
                    }
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }


        
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Polyline figure;
                    Brush userBrush;
                    Skeleton skeleton;

                    LayoutRoot.Children.Clear();
                    frame.CopySkeletonDataTo(this.frameSkeletons);
                   


                    for (int i = 0; i < this.frameSkeletons.Length; i++)
                    {
                        skeleton = this.frameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {


                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length];

                            //绘制头和躯干
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter
                                                                });
                            LayoutRoot.Children.Add(figure);

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制作腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制左臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                            LayoutRoot.Children.Add(figure);

                            float[] showHead = new float[]{skeleton.Joints[JointType.Head].Position.X,
                                    skeleton.Joints[JointType.Head].Position.Y,skeleton.Joints[JointType.Head].Position.Z,};
                            float[] showAnkleLeft = new float[]{skeleton.Joints[JointType.AnkleLeft].Position.X,
                                                                skeleton.Joints[JointType.AnkleLeft].Position.Y,skeleton.Joints[JointType.AnkleLeft].Position.Z,};
                            float[] showAnkleRight = new float[]{skeleton.Joints[JointType.AnkleRight].Position.X,skeleton.Joints[JointType.AnkleRight].Position.Y,skeleton.Joints[JointType.AnkleRight].Position.Z,};
                            float[] showElbowLeft = new float[]{skeleton.Joints[JointType.ElbowLeft].Position.X,
                                                                skeleton.Joints[JointType.ElbowLeft].Position.Y,skeleton.Joints[JointType.ElbowLeft].Position.Z,};
                            float[] showElbowRight = new float[]{skeleton.Joints[JointType.ElbowRight].Position.X, skeleton.Joints[JointType.ElbowRight].Position.Y,skeleton.Joints[JointType.ElbowRight].Position.Z,};
                            float[] showFootLeft = new float[]{skeleton.Joints[JointType.FootLeft].Position.X,
                                                                skeleton.Joints[JointType.FootLeft].Position.Y,skeleton.Joints[JointType.FootLeft].Position.Z,};
                            float[] showFootRight = new float[]{skeleton.Joints[JointType.FootRight].Position.X,
                                                                skeleton.Joints[JointType.FootRight].Position.Y,skeleton.Joints[JointType.FootRight].Position.Z,};

                            FileInfo file = new FileInfo(@"E:\kinect\head.txt");
                            StreamWriter sw = file.AppendText();
                            for (int j = 0; j < showHead.Length; j++)
                            {
                                sw.Write(showHead[j]);
                                sw.Write(' ');
                            }
                            sw.Dispose();
                        }
                    }

                    
                }
            }
        }
        
         private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {

            DepthImagePoint point = this.KinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.KinectDevice.DepthStream.Format);

            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
         } 
        
        
    }


}
