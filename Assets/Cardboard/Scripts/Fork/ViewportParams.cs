namespace TiltShift.Cardboard
{
    /// <summary>
    /// Viewport params container.
    /// </summary>
    internal class ViewportParams
    {
        public float Width { get; private set; }

        public float Height { get; private set; }

        public float XEyeOffset { get; private set; }

        public float YEyeOffset { get; private set; }

        /// <summary>
        /// Init viewport params.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="xEyeOffset">X eye offset.</param>
        /// <param name="yEyeOffset">Y eye offset.</param>
        public ViewportParams(float width, float height, float xEyeOffset, float yEyeOffset) 
        {
            Width = width;
            Height = height;
            XEyeOffset = xEyeOffset;
            YEyeOffset = yEyeOffset;
        }
    }
}