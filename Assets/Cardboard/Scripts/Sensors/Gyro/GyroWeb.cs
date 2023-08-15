using UnityEngine;
using UnityEngine.Android;

namespace TiltShift.Cardboard.Sensors
{
    public class GyroWeb : GyroBase
    {
        public override Quaternion Rotation =>
            Input.gyro.attitude * Quaternion.Euler(0, 0, -90);


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

       
    }
}