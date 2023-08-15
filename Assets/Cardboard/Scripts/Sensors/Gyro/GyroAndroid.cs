using UnityEngine;
using UnityEngine.Android;

namespace TiltShift.Cardboard.Sensors
{
    public class GyroAndroid : GyroBase
    {
        public override Quaternion Rotation =>
            QuaternionFromDeviceRotationVector(GetDeviceSensor("gamerotationvector"));

        private readonly string _pluginName = "xyz.tiltshift.unity.sensor.UnitySensorPlugin";

        private AndroidJavaObject _plugin;

        private void Awake()
        {
            _plugin =
                new AndroidJavaClass(_pluginName).CallStatic<AndroidJavaObject>(
                    "getInstance");

            if (_plugin != null)
            {
                _plugin.Call("startSensorListening", "gamerotationvector");
                _plugin.Call("setSamplingPeriod", 100);
            }
        }

        protected Quaternion QuaternionFromDeviceRotationVector(Vector3 v)
        {
            var r = new Quaternion(v.x, v.y, v.z, Mathf.Sqrt(1f - v.sqrMagnitude));
            return r * GetRotFix();
        }

        private Quaternion GetRotFix()
        {
            if (Screen.orientation == ScreenOrientation.Portrait)
                return Quaternion.identity;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft ||
                Screen.orientation == ScreenOrientation.LandscapeLeft)
                return Quaternion.Euler(0, 0, -90);

            if (Screen.orientation == ScreenOrientation.LandscapeRight)
                return Quaternion.Euler(0, 0, 90);

            if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                return Quaternion.Euler(0, 0, 180);

            return Quaternion.identity;
        }

        private Vector3 GetDeviceSensor(string type)
        {
            if (_plugin == null) return Vector3.zero;

            float[] sensorValue = _plugin.Call<float[]>("getSensorValues", type);

            return new Vector3(sensorValue[0], sensorValue[1], sensorValue[2]);
        }
    }
}