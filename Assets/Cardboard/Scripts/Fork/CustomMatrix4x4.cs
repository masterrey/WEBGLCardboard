using UnityEngine;

namespace TiltShift.Cardboard
{
    /// <summary>
    /// Modern matrix 4x4.
    /// </summary>
    internal class CustomMatrix4x4
    {
        private float[,] _m = new float[4, 4];

        /// <summary>
        /// Get identity matrix.
        /// </summary>
        /// <returns>Matrix.</returns>
        public static CustomMatrix4x4 Identity()
        {
            CustomMatrix4x4 ret = new CustomMatrix4x4();

            for (int j = 0; j < 4; ++j)
            {
                for (int i = 0; i < 4; ++i)
                {
                    ret._m[j, i] = (i == j) ? 1 : 0;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get zeros matrix.
        /// </summary>
        /// <returns>Matrix.</returns>
        public static CustomMatrix4x4 Zeros()
        {
            CustomMatrix4x4 ret = new CustomMatrix4x4();

            for (int j = 0; j < 4; ++j)
            {
                for (int i = 0; i < 4; ++i)
                {
                    ret._m[j, i] = 0;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <returns>Matrix.</returns>
        public static CustomMatrix4x4 Translation(float x, float y, float z)
        {
            CustomMatrix4x4 ret = CustomMatrix4x4.Identity();

            ret._m[3, 0] = x;
            ret._m[3, 1] = y;
            ret._m[3, 2] = z;

            return ret;
        }

        /// <summary>
        /// Get perspective matrix.
        /// </summary>
        /// <returns>Matrix.</returns>
        public static CustomMatrix4x4 Perspective(float[] fov, float zNear, float zFar)
        {
            CustomMatrix4x4 ret = CustomMatrix4x4.Zeros();

            float xLeft = -Mathf.Tan(fov[0] * Mathf.PI / 180.0f) * zNear;
            float xRight = Mathf.Tan(fov[1] * Mathf.PI / 180.0f) * zNear;
            float yBottom = -Mathf.Tan(fov[2] * Mathf.PI / 180.0f) * zNear;
            float yTop = Mathf.Tan(fov[3] * Mathf.PI / 180.0f) * zNear;

            float X = (2f * zNear) / (xRight - xLeft);
            float Y = (2f * zNear) / (yTop - yBottom);
            float A = (xRight + xLeft) / (xRight - xLeft);
            float B = (yTop + yBottom) / (yTop - yBottom);
            float C = (zNear + zFar) / (zNear - zFar);
            float D = (2f * zNear * zFar) / (zNear - zFar);

            ret._m[0, 0] = X;
            ret._m[2, 0] = A;
            ret._m[1, 1] = Y;
            ret._m[2, 1] = B;
            ret._m[2, 2] = C;
            ret._m[3, 2] = D;
            ret._m[2, 3] = -1f;

            return ret;
        }

        /// <summary>
        /// Translating to array.
        /// </summary>
        public float[] ToArray()
        {
            var result = new float[16];
            System.Buffer.BlockCopy(_m, 0, result, 0, 16);
            return result;
        }

        /// <summary>
        /// To Unity matrix.
        /// </summary>
        /// <returns>Unity Matrix4x4.</returns>
        public Matrix4x4 ToUnityMatrix()
        {
            var result = new Matrix4x4();
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    result[y, x] = _m[x, y];
                }
            }
            return result;
        }

    }
}