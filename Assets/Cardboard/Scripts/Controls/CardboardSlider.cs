using UnityEngine;
using UnityEngine.UI;

namespace TiltShift.Cardboard.Controls
{
    [RequireComponent(typeof(Slider))]
    public class CardboardSlider : CardboardControlBase
    {
        private BoxCollider _collider;

        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            SetCollider();
        }

        private void SetCollider()
        {
            _collider = gameObject.GetComponent<BoxCollider>();
            if (_collider != null)
            {
                return;
            }

            _collider = gameObject.AddComponent<BoxCollider>();

            var rect = GetComponent<RectTransform>().rect;
            _collider.size = new Vector3(rect.width, rect.height, 1f);
        }

        private void SetValue(float pos)
        {
            _slider.value = pos * (_slider.maxValue - _slider.minValue) + _slider.minValue;
        }

        public override void OnClick(Vector3 position)
        {
            var size = _collider.size;
            var localX = (position.x + size.x / 2f) / size.x;
            SetValue(localX);

        }
    }
}