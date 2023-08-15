using System.Collections.Generic;
using UnityEngine;

namespace TiltShift.Cardboard
{
    /// <summary>
    /// A Kalman filter implementation for <c>Vector3</c> values.
    /// </summary>
    public class KalmanFilterVector3
    {
        public const float DEFAULT_Q = 0.000001f;
        public const float DEFAULT_R = 0.01f;

        public const float DEFAULT_P = 1;

        private float _q;
        private float _r;
        private float _p = DEFAULT_P;
        private Vector3 _x;
        private float _k;

        public KalmanFilterVector3() : this(DEFAULT_Q) { }

        public KalmanFilterVector3(float aQ = DEFAULT_Q, float aR = DEFAULT_R)
        {
            _q = aQ;
            _r = aR;
        }

        public Vector3 Update(Vector3 measurement, float? newQ = null, float? newR = null)
        {
            if (newQ != null && _q != newQ)
            {
                _q = (float)newQ;
            }
            if (newR != null && _r != newR)
            {
                _r = (float)newR;
            }

            // update measurement.
            {
                _k = (_p + _q) / (_p + _q + _r);
                _p = _r * (_p + _q) / (_r + _p + _q);
            }

            // filter result back into calculation.
            Vector3 result = _x + (measurement - _x) * _k;
            _x = result;
            return result;
        }

        public Vector3 Update(List<Vector3> measurements, bool areMeasurementsNewestFirst = false, float? newQ = null, float? newR = null)
        {
            Vector3 result = Vector3.zero;
            int i = (areMeasurementsNewestFirst) ? measurements.Count - 1 : 0;

            while (i < measurements.Count && i >= 0)
            {
                // decrement or increment the counter.
                if (areMeasurementsNewestFirst)
                {
                    --i;
                }
                else
                {
                    ++i;
                }

                result = Update(measurements[i], newQ, newR);
            }

            return result;
        }

        public void Reset()
        {
            _p = 1;
            _x = Vector3.zero;
            _k = 0;
        }
    }
}