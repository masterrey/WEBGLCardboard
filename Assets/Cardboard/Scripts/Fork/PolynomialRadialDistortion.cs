using System.Collections.Generic;
using UnityEngine;

namespace TiltShift.Cardboard
{
    /// <summary>
    /// Polynomial radial distortion calculator.
    /// </summary>
    internal class PolynomialRadialDistortion
    {
        private List<float> _coeficients;

        /// <summary>
        /// Init calculator.
        /// </summary>
        /// <param name="coefs">List of coefs.</param>
        public PolynomialRadialDistortion(List<float> coefs)
        {
            _coeficients = coefs;
        }

        /// <summary>
        /// Get distortion factor.
        /// </summary>
        /// <param name="rSquared">Maybe radius.</param>
        /// <returns>Factor.</returns>
        public float DistortionFactor(float rSquared)
        {
            float rFactor = 1.0f;
            float distortionFactor = 1.0f;

            foreach (var ki in _coeficients)
            {
                rFactor *= rSquared;
                distortionFactor += ki * rFactor;
            }

            return distortionFactor;
        }

        /// <summary>
        /// Calculating distort radius.
        /// </summary>
        /// <param name="r">Radius.</param>
        /// <returns>Distort radius.</returns>
        private float DistortRadius(float r)
        {
            return r * DistortionFactor(r * r);
        }

        /// <summary>
        /// Function for distortion.
        /// </summary>
        /// <param name="p">Something params.</param>
        /// <returns>Distortion result (two params).</returns>
        public float[] Distort(float[] p)
        {
            float distortionFactor = DistortionFactor(p[0] * p[0] + p[1] * p[1]);

            return new float[2] 
            {
                distortionFactor* p[0],
                distortionFactor* p[1]
            };
        }

        /// <summary>
        /// Inverse distort.
        /// </summary>
        /// <param name="p">Something params.</param>
        /// <returns>Inverse distortion results (two params).</returns>
        public float[] DistortInverse(float[] p)
        {
            float radius = Mathf.Sqrt(p[0] * p[0] + p[1] * p[1]);

            if (Mathf.Abs(radius - 0.0f) < Mathf.Epsilon)
            {
                return new float[2];
            }

            float r0 = radius / 2.0f;
            float r1 = radius / 3.0f;
            float r2;
            float dr0 = radius - DistortRadius(r0);
            float dr1;
            while (Mathf.Abs(r1 - r0) > 0.0001f /** 0.1mm */)
            {
                dr1 = radius - DistortRadius(r1);
                r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
                r0 = r1;
                r1 = r2;
                dr0 = dr1;
            }

            return new float[] 
            { 
                (r1 / radius) * p[0], 
                (r1 / radius) * p[1] 
            };
        }
    }
}
