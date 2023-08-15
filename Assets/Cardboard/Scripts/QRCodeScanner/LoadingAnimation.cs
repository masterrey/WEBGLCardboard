using UnityEngine;

namespace TiltShift.UI
{
    public class LoadingAnimation : MonoBehaviour
    {
        public int CountSteps = 8;

        public float ChangeSpeed = 0.5f;

        private float _currentTime = 0;

        private int _currentState = 0;

        public bool InverseDirection = true;

        private void Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime < ChangeSpeed)
            {
                return;
            }

            _currentState++;

            if (_currentState >= CountSteps)
            {
                _currentState = 0;
            }

            _currentTime = 0;
            var value = -360f / (float)CountSteps * (float)_currentState * (InverseDirection ? -1f : 1f);
            transform.localRotation = Quaternion.Euler(0, 0, value);

        }
    }

}