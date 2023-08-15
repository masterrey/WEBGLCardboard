using UnityEngine;

namespace TiltShift.Cardboard.Sensors
{
    public class GyroEditor : GyroBase
    {
        private float _deltaX;

        private float _deltaY;

        private Vector3 _prevPosition;

        public override Quaternion Rotation => _rotation;

        private Quaternion _rotation;

        private void Awake()
        {
            _prevPosition = Input.mousePosition;
        }

        private void Update()
        {
            var delta = _prevPosition - Input.mousePosition;

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                _deltaX += delta.x;
                _deltaY += delta.y;

                _deltaX = _deltaX > 360f ? _deltaX - 360f : _deltaX;
                _deltaX = _deltaX < -360f ? _deltaX + 360f : _deltaX;

                _deltaY = Mathf.Clamp(_deltaY, -90f, 90);

                _rotation = Quaternion.Euler(_deltaY, -_deltaX, 0);
            }

            _prevPosition = Input.mousePosition;
        }
    }
}
