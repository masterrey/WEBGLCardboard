using Google.Protobuf;
using System.Linq;
using UnityEngine;

namespace TiltShift.Cardboard
{
    /// <summary>
    /// Mechanism for calcualtin cardboard rendering info.
    /// </summary>
    public class LensDistortion
    {
        private float _defaultBorderSizeMeters = 0.003f;
        private DeviceParams _deviceParams;

        private CustomMatrix4x4[] _projectionMatrix = new CustomMatrix4x4[2];
        private DistortionMesh _leftMesh;
        private DistortionMesh _rightMesh;
        private PolynomialRadialDistortion _distortion;

        private Vector3[] _eyePosition = new Vector3[2];

        private float[][] _fov = new float[2][];

        private float _screenWidthMeters;
        private float _screenHeightMeters;

        /// <summary>
        /// Mechanism for calculating cardboard rendering info.
        /// </summary>
        /// <param name="encodedDeviceParamsBase64">Payload from link of QRCode.</param>
        /// <param name="displayWidth">Screen width.</param>
        /// <param name="displayHeight">Screen height.</param>
        /// <param name="dpi">Screen dpi.</param>
        public LensDistortion(string encodedDeviceParamsBase64, int displayWidth, int displayHeight, float dpi)
        {
            UpdateDevice(encodedDeviceParamsBase64);

            UpdateSize(displayWidth, displayHeight, dpi);
        }

        /// <summary>
        /// Get camera projection matrix for eye.
        /// </summary>
        /// <param name="eye">Eye.</param>
        /// <returns>Unity projection matrix.</returns>
        public Matrix4x4 GetProjectionMatrix(CardboardEye eye)
        {
            return _projectionMatrix[(int)eye].ToUnityMatrix();
        }

        /// <summary>
        /// Get camera position for eye.
        /// </summary>
        /// <param name="eye">Eye.</param>
        /// <returns>Position of camera.</returns>
        public Vector3 GetEyePosition(CardboardEye eye)
        {
            return _eyePosition[(int)eye];
        }

        /// <summary>
        /// Update sizes for calculating.
        /// </summary>
        /// <param name="displayWidth">Screen width in pixels.</param>
        /// <param name="displayHeight">Screen height in pixels.</param>
        /// <param name="dpi">Screen dpi.</param>
        public void UpdateSize(int displayWidth, int displayHeight, float dpi)
        {
            _eyePosition[(int)CardboardEye.Left] = new Vector3(-_deviceParams.InterLensDistance * 0.5f, 0f, 0f);
            _eyePosition[(int)CardboardEye.Right] = new Vector3(_deviceParams.InterLensDistance * 0.5f, 0f, 0f);

            _distortion = new PolynomialRadialDistortion(_deviceParams.DistortionCoefficients.ToList());

            _screenWidthMeters = GetSizeInMeters(displayWidth, dpi);
            _screenHeightMeters = GetSizeInMeters(displayHeight, dpi);

            UpdateParams();
        }


        /// <summary>
        /// Update device params.
        /// </summary>
        /// <param name="encodedDeviceParamsBase64">Payload from link of QRCode.</param>
        private void UpdateDevice(string encodedDeviceParamsBase64)
        {
            _deviceParams = DeviceParams.Parser.ParseFrom(ByteString.FromBase64(encodedDeviceParamsBase64));

            Debug.Log(_deviceParams.SummaryInfoString());
        }

        /// <summary>
        /// Translating to meters.
        /// </summary>
        /// <param name="source">Length in pixels.</param>
        /// <param name="dpi">DPI of screen.</param>
        /// <returns>Size in meters.</returns>
        private float GetSizeInMeters(float source, float dpi)
        {
            var meters = (source / dpi) * (2.54f / 100f);
            return meters;
        }

        /// <summary>
        /// Calculate mesh for one eye.
        /// </summary>
        /// <param name="eye">Eye.</param>
        /// <returns>Unity mesh.</returns>
        public Mesh GetDistortionMesh(CardboardEye eye)
        {
            return eye == CardboardEye.Left ? _leftMesh.UnityMesh : _rightMesh.UnityMesh;
        }

        /// <summary>
        /// Calculating meshes.
        /// </summary>
        private void UpdateParams()
        {
            _fov[(int)CardboardEye.Left] = CalculateFov(_deviceParams, _distortion);

            // Mirror fov for right eye.
            _fov[(int)CardboardEye.Right] = ((float[])_fov[(int)CardboardEye.Left].Clone());
            _fov[(int)CardboardEye.Right][0] = _fov[(int)CardboardEye.Left][1];
            _fov[(int)CardboardEye.Right][1] = _fov[(int)CardboardEye.Left][0];

            _projectionMatrix[(int)CardboardEye.Left] = CustomMatrix4x4.Perspective(_fov[(int)CardboardEye.Left], 0.1f, 100f);
            _projectionMatrix[(int)CardboardEye.Right] = CustomMatrix4x4.Perspective(_fov[(int)CardboardEye.Right], 0.1f, 100f);

            _leftMesh = CreateDistortionMesh(CardboardEye.Left, _deviceParams, _distortion, _fov[(int)CardboardEye.Left]);

            _rightMesh = CreateDistortionMesh(CardboardEye.Right, _deviceParams, _distortion, _fov[(int)CardboardEye.Right]);
        }

        /// <summary>
        /// Calculating field of view.
        /// </summary>
        /// <param name="deviceParams">Device parametrs.</param>
        /// <param name="distortion">List of coeficients of polynomial radial distortion.</param>
        /// <returns>Field of view.</returns>
        private float[] CalculateFov(DeviceParams deviceParams, PolynomialRadialDistortion distortion)
        {
            // This in in degrees.
            var deviceFov = new float[4]
            {
                deviceParams.LeftEyeFieldOfViewAngles[0],
                deviceParams.LeftEyeFieldOfViewAngles[1],
                deviceParams.LeftEyeFieldOfViewAngles[2],
                deviceParams.LeftEyeFieldOfViewAngles[3],
            };

            float eyeToScreenDistance = deviceParams.ScreenToLensDistance;
            float outerDistance = (_screenWidthMeters - deviceParams.InterLensDistance) / 2.0f;
            float innerDistance = deviceParams.InterLensDistance / 2.0f;
            float bottomDistance = GetYEyeOffsetMeters(deviceParams);
            float topDistance = _screenHeightMeters - bottomDistance;

            float outerAngle = Mathf.Atan(distortion.Distort(new float[2] { outerDistance / eyeToScreenDistance, 0 })[0]) * 180.0f / Mathf.PI;
            float innerAngle = Mathf.Atan(distortion.Distort(new float[2] { innerDistance / eyeToScreenDistance, 0 })[0]) * 180.0f / Mathf.PI;
            float bottomAngle = Mathf.Atan(distortion.Distort(new float[2] { 0, bottomDistance / eyeToScreenDistance })[1]) * 180.0f / Mathf.PI;
            float topAngle = Mathf.Atan(distortion.Distort(new float[2] { 0, topDistance / eyeToScreenDistance })[1]) * 180.0f / Mathf.PI;

            return new float[4]
            {
                Mathf.Min(outerAngle, deviceFov[0]),
                Mathf.Min(innerAngle, deviceFov[1]),
                Mathf.Min(bottomAngle, deviceFov[2]),
                Mathf.Min(topAngle, deviceFov[3]),
            };
        }

        /// <summary>
        /// Get offset by alignment.
        /// </summary>
        /// <param name="deviceParams">Device params.</param>
        /// <returns>Offset in meters.</returns>
        private float GetYEyeOffsetMeters(DeviceParams deviceParams)
        {
            switch (deviceParams.VerticalAlignment)
            {
                case DeviceParams.Types.VerticalAlignmentType.Center:
                default:
                    return _screenHeightMeters / 2.0f;
                case DeviceParams.Types.VerticalAlignmentType.Bottom:
                    return deviceParams.TrayToLensDistance - _defaultBorderSizeMeters;
                case DeviceParams.Types.VerticalAlignmentType.Top:
                    return _screenHeightMeters - deviceParams.TrayToLensDistance - _defaultBorderSizeMeters;
            }
        }

        /// <summary>
        /// Creating distortion mesh by params.
        /// </summary>
        /// <param name="eye">Eye.</param>
        /// <param name="deviceParams">Device parametrs.</param>
        /// <param name="distortion">List of coeficients of polynomial radial distortion.</param>
        /// <param name="fov">Field of view.</param>
        /// <returns>Internal mesh for next using in Unity.</returns>
        private DistortionMesh CreateDistortionMesh(CardboardEye eye, DeviceParams deviceParams,
            PolynomialRadialDistortion distortion, float[] fov)
        {
            var viewportParams = CalculateViewportParameters(eye, deviceParams, fov);

            return new DistortionMesh(distortion, viewportParams.Item1, viewportParams.Item2);
        }

        /// <summary>
        /// Calculate texture and screen params for eye.
        /// </summary>
        /// <param name="eye">Type of eye.</param>
        /// <param name="deviceParams">Device parametrs.</param>
        /// <param name="fov">Field of view.</param>
        /// <returns>Two viewport params (screen params and texture params).</returns>
        private (ViewportParams, ViewportParams) CalculateViewportParameters(CardboardEye eye, DeviceParams deviceParams, float[] fov)
        {            
            var scrParamsWidth = _screenWidthMeters / deviceParams.ScreenToLensDistance;
            var scrParamsHeight = _screenHeightMeters / deviceParams.ScreenToLensDistance;

            var scrParamsXEyeOffset = eye == CardboardEye.Left 
                ? ((_screenWidthMeters - deviceParams.InterLensDistance) / 2f) / deviceParams.ScreenToLensDistance
                : ((_screenWidthMeters + deviceParams.InterLensDistance) / 2f) / deviceParams.ScreenToLensDistance;
         
            var scrParamsYEyeOffset = GetYEyeOffsetMeters(deviceParams) / deviceParams.ScreenToLensDistance;

            ViewportParams screenParams = new ViewportParams(scrParamsWidth, scrParamsHeight, scrParamsXEyeOffset, scrParamsYEyeOffset);

            var texParamsWidth = Mathf.Tan(fov[0] * Mathf.PI / 180f) + Mathf.Tan(fov[1] * Mathf.PI / 180f);
            var texParamsHeight = Mathf.Tan(fov[2] * Mathf.PI / 180f) + Mathf.Tan(fov[3] * Mathf.PI / 180f);

            var texParamsXEyeOffset = Mathf.Tan(fov[0] * Mathf.PI / 180f);
            var texParamsYEyeOffset = Mathf.Tan(fov[2] * Mathf.PI / 180f);

            ViewportParams textureParams = new ViewportParams(texParamsWidth, texParamsHeight, texParamsXEyeOffset, texParamsYEyeOffset);

            return (screenParams, textureParams);
        }
    }
}
