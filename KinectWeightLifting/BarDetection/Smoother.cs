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

namespace BarDetection
{/// <summary>
 /// Smooths the given values to provide a consistent result.
 /// EXAMPLE:
 /// // OnStart():
 /// Smoother smoother = new Smoother();
 /// // OnUpdate():
 /// double value = ... // some raw data.
 /// double result = smoother.Smooth(value);
 /// </summary>
    public class Smoother
    {
        #region Constants

        private const int HISTORY_SIZE = 5;
        private const double MAX_MARGIN = 5;

        #endregion

        #region Members

        private double[] _historyCorrectAxes;
        private int _counterCorrectAxes = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the Smoother class.
        /// </summary>
        public Smoother()
        {
            _historyCorrectAxes = new double[HISTORY_SIZE];
        }

        /// <summary>
        /// Creates a new instance of the Smoother class with the specified history size and smoothing step.
        /// </summary>
        /// <param name="historySize">The size of the history to check to compute a new value. Defaults 10.
        /// WARNING: using a big number may cause some delay.</param>
        /// <param name="smoothingStep">This is used only in Strict Smoothing. It determines the maximum difference between the previous value and the new one. Defaults to 5.
        /// WARNING: using small numbers will cause some delay.</param>
        public Smoother(int historySize)
        {
            _historyCorrectAxes = new double[historySize];
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Smooths the specified Double value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="strict">Specifies whether simple or strict smoothing is used.</param>
        /// <returns>The smoothed result.</returns>
        public double Smooth(double value, bool strict = false)
        {
            return strict ? StrictSmooth(value) : SimpleSmooth(value);
        }

        /// <summary>
        /// Smooths the specified Float value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="strict">Specifies whether simple or strict smoothing is used.</param>
        /// <returns>The smoothed result.</returns>
        public float Smooth(float value, bool strict = false)
        {
            double result = Smooth((double)value);

            return (float)result;
        }

        /// <summary>
        /// Smooths the specified Integer value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="strict">Specifies whether simple or strict smoothing is used.</param>
        /// <returns>The smoothed result.</returns>
        public int Smooth(int value, bool strict = false)
        {
            double result = Smooth((double)value);

            return (int)result;
        }

        /// <summary>
        /// Smooths the specified Boolean value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="strict">Specifies whether simple or strict smoothing is used.</param>
        /// <returns>The smoothed result.</returns>
        public bool Smooth(bool value, bool strict = false)
        {
            double result = Smooth(value ? 1.0 : 0.0, strict);

            return result == 0.0 ? false : true;
        }

        #endregion

        #region Algorithms

        private double SimpleSmooth(double value)
        {
            _historyCorrectAxes[_counterCorrectAxes % _historyCorrectAxes.Length] = value;
            _counterCorrectAxes++;

            double sum = value;

            if (_counterCorrectAxes >= _historyCorrectAxes.Length)
            {
                _counterCorrectAxes = _counterCorrectAxes % _historyCorrectAxes.Length + _historyCorrectAxes.Length;
                sum = 0;

                foreach (double x in _historyCorrectAxes)
                {
                    sum += x;
                }

                sum = sum / _historyCorrectAxes.Length;
            }

            return sum;
        }

        private double StrictSmooth(double value)
        {
            double sum = value;

            if (_counterCorrectAxes >= _historyCorrectAxes.Length)
            {
                foreach (double x in _historyCorrectAxes)
                {
                    sum += x;
                }

                sum = sum / _historyCorrectAxes.Length;

                if (value > sum)
                {
                    value = Math.Min(sum + MAX_MARGIN, value);
                }
                else
                {
                    value = Math.Max(sum - MAX_MARGIN, value);
                }

                return SimpleSmooth(value);
            }

            return sum;
        }

        #endregion
    }

}

