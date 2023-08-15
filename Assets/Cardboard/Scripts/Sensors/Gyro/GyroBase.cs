using UnityEngine;

namespace TiltShift.Cardboard.Sensors
{
    public abstract class GyroBase : MonoBehaviour
    {
        public virtual Quaternion Rotation => Quaternion.identity;

    }
}
