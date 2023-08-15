using UnityEngine;

namespace TiltShift.Cardboard.Sensors
{
    public class GyroIOS : GyroBase
    {
        public override Quaternion Rotation => Input.gyro.attitude;

        private void Awake()
        {
            Input.gyro.enabled = true;
        }
    }
}
