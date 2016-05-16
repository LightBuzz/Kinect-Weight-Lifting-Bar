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

using Microsoft.Kinect;
using System;
using System.Windows.Media.Media3D;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Provides some commonly used Mathematical functions.
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Calculates the angle between the specified points.
        /// </summary>
        /// <param name="center">The center of the angle.</param>
        /// <param name="start">The start of the angle.</param>
        /// <param name="end">The end of the angle.</param>
        /// <returns>The angle, in degrees.</returns>
        public static double Angle(this CameraSpacePoint center, CameraSpacePoint start, CameraSpacePoint end)
        {
            Vector3D first = new Vector3D(start.X, start.Y, start.Z) - new Vector3D(center.X, center.Y, center.Z);
            Vector3D second = new Vector3D(end.X, end.Y, end.Z) - new Vector3D(center.X, center.Y, center.Z);

            return Vector3D.AngleBetween(first, second);
        }

        /// <summary>
        /// Returns the length of the segment defined by the specified points.
        /// </summary>
        /// <param name="point1">The first point (start of the segment).</param>
        /// <param name="point2">The second point (end of the segment).</param>
        /// <returns>The length of the segment (in meters).</returns>
        public static double Length(this CameraSpacePoint point1, CameraSpacePoint point2)
        {
            return Math.Sqrt(
                Math.Pow(point1.X - point2.X, 2) +
                Math.Pow(point1.Y - point2.Y, 2) +
                Math.Pow(point1.Z - point2.Z, 2)
            );
        }
    }
}
