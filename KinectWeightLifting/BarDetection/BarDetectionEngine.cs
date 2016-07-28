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

using System;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;

namespace BarDetection
{
    public class BarDetectionEngine
    {
        private readonly double HEIGHT_DIFFERENCE = 0.03;

        private DepthSpacePoint[] _depthPoints = null;

        public CoordinateMapper CoordinateMapper { get; set; }

        public int ColorWidth { get; set; }

        public int ColorHeight { get; set; }

        public int DepthWidth { get; set; }

        public int DepthHeight { get; set; }

        public event EventHandler<BarDetectionResult> BarDetected;

        public BarDetectionEngine(CoordinateMapper coordinateMapper, int colorWidth, int colorHeight, int depthWidth, int depthHeight)
        {
            CoordinateMapper = coordinateMapper;
            ColorWidth = colorWidth;
            ColorHeight = colorHeight;
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;

            _depthPoints = new DepthSpacePoint[ColorWidth * ColorHeight];
        }

        public unsafe void Update(ushort[] depthData, byte[] bodyIndexData, Body body)
        {
            double handLength = 0.0;
            double barLength = 0.0;
            double barHeight = 0.0;
            double angle = 0.0;

            Joint shoulderLeft = body.Joints[JointType.ShoulderLeft];
            Joint shoulderRight = body.Joints[JointType.ShoulderRight];
            Joint chest = body.Joints[JointType.SpineShoulder];
            Joint waist = body.Joints[JointType.SpineBase];
            Joint elbowLeft = body.Joints[JointType.ElbowLeft];
            Joint elbowRight = body.Joints[JointType.ElbowRight];
            Joint handLeft = body.Joints[JointType.HandLeft];
            Joint handRight = body.Joints[JointType.HandRight];
            Joint footLeft = body.Joints[JointType.FootLeft];
            Joint footRight = body.Joints[JointType.FootRight];

            if (waist.TrackingState == TrackingState.NotTracked) return;

            if (waist.Position.Z < 2.0f || waist.Position.Z > 4.5f) return;

            if (shoulderLeft.TrackingState != TrackingState.NotTracked && shoulderRight.TrackingState != TrackingState.NotTracked &&
                elbowLeft.TrackingState != TrackingState.NotTracked && elbowRight.TrackingState != TrackingState.NotTracked &&
                handLeft.TrackingState != TrackingState.NotTracked && handRight.TrackingState != TrackingState.NotTracked)
            {
                handLength =
                    shoulderLeft.Position.Length(shoulderRight.Position) +
                    shoulderLeft.Position.Length(elbowLeft.Position) +
                    shoulderRight.Position.Length(elbowRight.Position) +
                    elbowLeft.Position.Length(handLeft.Position) +
                    elbowRight.Position.Length(handRight.Position);
            }

            CoordinateMapper.MapColorFrameToDepthSpace(depthData, _depthPoints);

            fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = _depthPoints)
            {
                int minimumX = int.MaxValue;
                int maximumX = int.MinValue;
                int minimumY = int.MaxValue;
                int maximumY = int.MinValue;
                ushort minimumDistance = 0;
                ushort maximumdistance = 0;

                for (int colorIndex = 0; colorIndex < _depthPoints.Length; ++colorIndex)
                {
                    float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                    float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;

                    if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                        !float.IsNegativeInfinity(colorMappedToDepthY))
                    {
                        int depthX = (int)(colorMappedToDepthX + 0.5f);
                        int depthY = (int)(colorMappedToDepthY + 0.5f);

                        if ((depthX >= 0) && (depthX < DepthWidth) && (depthY >= 0) && (depthY < DepthHeight))
                        {
                            int depthIndex = (depthY * DepthWidth) + depthX;
                            ushort depth = depthData[depthIndex];

                            if (bodyIndexData[depthIndex] != 0xff)
                            {
                                if (depthX < minimumX)
                                {
                                    minimumX = depthX;
                                    minimumY = depthY;
                                    minimumDistance = depth;
                                }
                                if (depthX > maximumX)
                                {
                                    maximumX = depthX;
                                    maximumY = depthY;
                                    maximumdistance = depth;
                                }
                                continue;
                            }
                        }
                    }
                }

                DepthSpacePoint depthMinimum = new DepthSpacePoint
                {
                    X = minimumX,
                    Y = minimumY
                };

                DepthSpacePoint depthMaximum = new DepthSpacePoint
                {
                    X = maximumX,
                    Y = maximumY
                };

                CameraSpacePoint cameraMinimum = CoordinateMapper.MapDepthPointToCameraSpace(depthMinimum, minimumDistance);
                CameraSpacePoint cameraMaximum = CoordinateMapper.MapDepthPointToCameraSpace(depthMaximum, maximumdistance);

                ColorSpacePoint colorMinimum = CoordinateMapper.MapDepthPointToColorSpace(depthMinimum, minimumDistance);
                ColorSpacePoint colorMaximum = CoordinateMapper.MapDepthPointToColorSpace(depthMaximum, maximumdistance);

                CameraSpacePoint cameraTrail = new CameraSpacePoint
                {
                    X = (cameraMinimum.X + cameraMaximum.X) / 2f,
                    Y = (cameraMinimum.Y + cameraMaximum.Y) / 2f,
                    Z = (cameraMinimum.Z + cameraMaximum.Z) / 2f
                };

                ColorSpacePoint colorTrail = new ColorSpacePoint
                {
                    X = (colorMinimum.X + colorMaximum.X) / 2f,
                    Y = (colorMinimum.Y + colorMaximum.Y) / 2f
                };

                DepthSpacePoint depthTrail = new DepthSpacePoint
                {
                    X = (depthMinimum.X + depthMaximum.X) / 2f,
                    Y = (depthMinimum.Y + depthMaximum.Y) / 2f
                };

                CameraSpacePoint feet = new CameraSpacePoint
                {
                    X = (footLeft.Position.X + footRight.Position.X) / 2f,
                    Y = (footLeft.Position.Y + footRight.Position.Y) / 2f,
                    Z = (footLeft.Position.Z + footRight.Position.Z) / 2f
                };

                CameraSpacePoint projection = new CameraSpacePoint
                {
                    X = cameraTrail.X,
                    Y = feet.Y,
                    Z = cameraTrail.Z
                };

                barLength = cameraMinimum.Length(cameraMaximum);
                barHeight = cameraTrail.Length(projection);

                angle = cameraMinimum.Angle(cameraMaximum, new CameraSpacePoint { X = cameraMaximum.X, Y = cameraMinimum.Y, Z = (cameraMaximum.Z + cameraMinimum.Z) / 2f });
                
                if (angle > 180.0)
                {
                    angle = 360.0 - angle;
                }

                if (cameraMinimum.Y < cameraMaximum.Y)
                {
                    angle = -angle;
                }

                if (barLength > handLength && Math.Abs(angle) < 25.0)
                {
                    BarDetectionResult result = new BarDetectionResult
                    {
                        Minimum = new MultiPoint
                        {
                            CameraPoint = cameraMinimum,
                            ColorPoint = colorMinimum,
                            DepthPoint = depthMinimum
                        },
                        Maximum = new MultiPoint
                        {
                            CameraPoint = cameraMaximum,
                            ColorPoint = colorMaximum,
                            DepthPoint = depthMaximum
                        },
                        Trail = new MultiPoint
                        {
                            CameraPoint = cameraTrail,
                            ColorPoint = colorTrail,
                            DepthPoint = depthTrail
                        },
                        BarHeight = barHeight + HEIGHT_DIFFERENCE,
                        BarLength = barLength,
                        Angle = angle
                    };

                    BarDetected?.Invoke(this, result);
                }
            }
        }
    }
}
