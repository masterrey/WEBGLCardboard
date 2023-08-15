using UnityEngine;
using System.Collections;
using TiltShift.Cardboard.Sensors;

namespace TiltShift.Cardboard.Camera
{
    public class GyroCameraController : MonoBehaviour
    {
        private float _initialYAngle = 0f;
        private float _appliedGyroYAngle = 0f;
        private float _calibrationYAngle = 0f;
        private Transform _rawGyroRotation;
        private float _tempSmoothing;

        [SerializeField]
        private float _smoothing = 1f;

        private KalmanFilterVector3 _kalmanUp;
        private KalmanFilterVector3 _kalmanForward;

        private GyroBase _gyroSensor;

        public Canvas EditorCanvas;

        private void Awake()
        {
#if UNITY_EDITOR
            _gyroSensor = gameObject.AddComponent<GyroEditor>();
            Instantiate(EditorCanvas);
#elif UNITY_IPHONE
            _gyroSensor = gameObject.AddComponent<GyroIOS>();
#elif UNITY_ANDROID
            _gyroSensor = gameObject.AddComponent<GyroAndroid>();
#elif UNITY_WEBGL
            _gyroSensor = gameObject.AddComponent<GyroWeb>();
#endif
        }

        private IEnumerator Start()
        {
            Input.gyro.enabled = true;

            _kalmanForward = new KalmanFilterVector3();
            _kalmanUp = new KalmanFilterVector3();

            _initialYAngle = transform.localEulerAngles.y;

            _rawGyroRotation = new GameObject("GyroRaw").transform;
            _rawGyroRotation.position = transform.position;
            _rawGyroRotation.rotation = transform.localRotation;

            // Wait until gyro is active, then calibrate to reset starting rotation.
            yield return new WaitForEndOfFrame();

            StartCoroutine(CalibrateYAngle());
        }

        private void Update()
        {
            ApplyGyroRotation();
            ApplyCalibration();

            transform.localRotation = Quaternion.Slerp(transform.localRotation, _rawGyroRotation.rotation, _smoothing);
        }

        private IEnumerator CalibrateYAngle()
        {
            _tempSmoothing = _smoothing;
            _smoothing = 1;
            _calibrationYAngle = _appliedGyroYAngle - _initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
            yield return null;
            _smoothing = _tempSmoothing;
        }

        private void ApplyGyroRotation()
        {
            var vectorUp = _gyroSensor.Rotation * Vector3.up;
            var vectorForward = _gyroSensor.Rotation * Vector3.forward;
            _rawGyroRotation.rotation = Quaternion.LookRotation(vectorForward, vectorUp);
#if !UNITY_EDITOR
            _rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self); // Swap "handedness" of quaternion from gyro.
            _rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); // Rotate to make sense as a camera pointing out the back of your device.
#endif
            _appliedGyroYAngle = _rawGyroRotation.eulerAngles.y; // Save the angle around y axis for use in calibration.
        }

        private void ApplyCalibration()
        {
            _rawGyroRotation.Rotate(0f, -_calibrationYAngle, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
        }

        public void SetEnabled(bool value)
        {
            enabled = true;
            StartCoroutine(CalibrateYAngle());
        }
    }
}