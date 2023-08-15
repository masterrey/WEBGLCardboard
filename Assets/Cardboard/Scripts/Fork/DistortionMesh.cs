using System.Collections.Generic;
using UnityEngine;

namespace TiltShift.Cardboard
{
    /// <summary>
    /// Class for calculating distortion mesh.
    /// </summary>
    internal class DistortionMesh
    {
        private int _resolution = 50;

        /// <summary>
        /// Unity mesh for rendering.
        /// </summary>
        public Mesh UnityMesh { get; private set; }

        /// <summary>
        /// Init calculator distortion mesh.
        /// </summary>
        /// <param name="distortion">Coeficients of polynom.</param>
        /// <param name="screenParams">Screen viewport params.</param>
        /// <param name="textureParams">Texture viewport params.</param>
        public DistortionMesh(PolynomialRadialDistortion distortion, ViewportParams screenParams, ViewportParams textureParams)
        {
            UnityMesh = new Mesh();

            var vertexData = new Vector3[_resolution * _resolution];
            var uvs = new List<Vector2>();
            var indicesTri = new List<int>();

            float u_screen, v_screen, u_texture, v_texture;

            var p_texture = new float[2];
            var p_screen = new float[2];

            for (int row = 0; row < _resolution; row++)
            {
                for (int col = 0; col < _resolution; col++)
                {
                    // vertices.
                    u_texture = ((float)(col) / (_resolution - 1));
                    v_texture = ((float)(row) / (_resolution - 1));

                    p_texture[0] = u_texture * textureParams.Width - textureParams.XEyeOffset;
                    p_texture[1] = v_texture * textureParams.Height - textureParams.YEyeOffset;

                    p_screen = distortion.DistortInverse(p_texture);

                    u_screen = (p_screen[0] + screenParams.XEyeOffset) / screenParams.Width;
                    v_screen = (p_screen[1] + screenParams.YEyeOffset) / screenParams.Height;

                    var index = (row * _resolution + col);

                    vertexData[index].x = 2 * u_screen - 1;
                    vertexData[index].y = 2 * v_screen - 1;

                    // uvs.
                    uvs.Add(new Vector2((float)col / (float)(_resolution - 1), (float)row / (float)(_resolution - 1)));

                    // indices.
                    if (row < _resolution - 1 && col < _resolution - 1)
                    {
                        indicesTri.Add(row * _resolution + col);
                        indicesTri.Add((row + 1) * _resolution + col);
                        indicesTri.Add(row * _resolution + col + 1);

                        indicesTri.Add(row * _resolution + col + 1);
                        indicesTri.Add((row + 1) * _resolution + col);
                        indicesTri.Add((row + 1) * _resolution + col + 1);
                    }
                }
            }

            UnityMesh.vertices = vertexData;
            UnityMesh.triangles = indicesTri.ToArray();
            UnityMesh.uv = uvs.ToArray();
            UnityMesh.RecalculateBounds();
            UnityMesh.UploadMeshData(true);
        }
    }
}
