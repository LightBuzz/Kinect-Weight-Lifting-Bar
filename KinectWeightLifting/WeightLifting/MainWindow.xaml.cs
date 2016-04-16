//
// Copyright (c) LightBuzz Software.
// All rights reserved.
//
// http://lightbuzz.com
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
// AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using BarDetection;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WeightLifting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants

        private readonly double ELLIPSE_SIZE = 8.0;

        #endregion

        #region Members

        private KinectSensor _sensor = null;
        private CoordinateMapper _coordinateMapper = null;
        private MultiSourceFrameReader _reader = null;
        private BarDetectionEngine _barDetectionEngine = null;
        private IList<Body> _bodyData = null;
        private Body _body = null;
        private WriteableBitmap _bitmap = null;
        private byte[] _colorData = null;
        private ushort[] _depthData = null;
        private byte[] _bodyIndexData = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _coordinateMapper = _sensor.CoordinateMapper;

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                int colorWidth = _sensor.ColorFrameSource.FrameDescription.Width;
                int colorHeight = _sensor.ColorFrameSource.FrameDescription.Height;
                int depthWidth = _sensor.DepthFrameSource.FrameDescription.Width;
                int depthHeight = _sensor.DepthFrameSource.FrameDescription.Height;

                _bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                _bodyData = new Body[_sensor.BodyFrameSource.BodyCount];
                _colorData = new byte[colorWidth * colorHeight * 4];
                _depthData = new ushort[depthWidth * depthHeight];
                _bodyIndexData = new byte[depthWidth * depthHeight];

                _barDetectionEngine = new BarDetectionEngine(_coordinateMapper, colorWidth, colorHeight, depthWidth, depthHeight);
                _barDetectionEngine.BarDetected += BarDetectionEngine_BarDetected;

                camera.Source = _bitmap;
            }
        }

        #endregion

        #region Event Handlers

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
                using (var bodyFrame = frame.BodyFrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        bodyFrame.GetAndRefreshBodyData(_bodyData);

                        _body = _bodyData.Where(b => b.IsTracked).OrderBy(b => b.Joints[JointType.SpineBase].Position.Z).FirstOrDefault();

                        if (_body != null)
                        {
                            float distance = _body.Joints[JointType.SpineBase].Position.Z;

                            if (distance < 2.0)
                            {
                                sensorInfo.Visibility = Visibility.Visible;
                                sensorInfo.Text = "MOVE BACKWARDS";
                            }
                            else
                            {
                                sensorInfo.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }

                using (var colorFrame = frame.ColorFrameReference.AcquireFrame())
                {
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Bgra);

                        _bitmap.Lock();
                        Marshal.Copy(_colorData, 0, _bitmap.BackBuffer, _colorData.Length);
                        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                        _bitmap.Unlock();
                    }
                }

                using (var depthFrame = frame.DepthFrameReference.AcquireFrame())
                using (var bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame())
                {
                    if (depthFrame != null && bodyIndexFrame != null && _body != null)
                    {
                        depthFrame.CopyFrameDataToArray(_depthData);
                        bodyIndexFrame.CopyFrameDataToArray(_bodyIndexData);

                        _barDetectionEngine.Update(_depthData, _bodyIndexData, _body);
                    }
                }
            }
        }

        private void BarDetectionEngine_BarDetected(object sender, BarDetectionResult e)
        {
            if (e != null)
            {
                ColorSpacePoint colorPointMin = e.Minimum.ColorPoint;
                ColorSpacePoint colorPointMax = e.Maximum.ColorPoint;
                ColorSpacePoint colorPointTrail = e.Trail.ColorPoint;

                if (!float.IsInfinity(colorPointMin.X) && !float.IsInfinity(colorPointMin.Y) &&
                    !float.IsInfinity(colorPointMax.X) && !float.IsInfinity(colorPointMax.Y))
                {
                    // Bar line
                    horizontalLine.Visibility = Visibility.Visible;
                    horizontalLine.Width = Math.Abs(colorPointMax.X - colorPointMin.X);

                    Canvas.SetLeft(horizontalLine, colorPointMin.X);
                    Canvas.SetTop(horizontalLine, colorPointTrail.Y - horizontalLine.ActualHeight / 2.0);

                    // Vertical line
                    verticalLine.Visibility = Visibility.Visible;
                    verticalLine.Height = canvas.ActualHeight - colorPointTrail.Y - (horizontalLine.ActualHeight / 2.0);
                    verticalLine.Text = e.BarHeight.ToString("N2");

                    Canvas.SetLeft(verticalLine, colorPointTrail.X - verticalLine.ActualWidth / 2.0);
                }
                else
                {
                    horizontalLine.Visibility = Visibility.Collapsed;
                    verticalLine.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}
