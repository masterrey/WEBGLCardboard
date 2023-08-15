using UnityEngine;
using UnityEngine.UI;

namespace TiltShift.Cardboard.Controls
{
    [RequireComponent(typeof(Button))]
    public class CardboardButton : CardboardControlBase
    {
        private Button _button;
        private ColorBlock _colorBlock;
        private ColorBlock _sourceColorBlock;

        private bool _hovered = false;
        private bool _interactable => _button.interactable;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_button == null)
            {
                return;
            }

            _button.enabled = false;

            _colorBlock = _sourceColorBlock = _button.colors;
            _colorBlock.disabledColor = _sourceColorBlock.normalColor;

            SetCollider();
        }

        private void SetCollider()
        {
            if (gameObject.GetComponent<BoxCollider>() != null)
            {
                return;
            }

            var boxCollider = gameObject.AddComponent<BoxCollider>();

            var rect = GetComponent<RectTransform>().rect;
            boxCollider.size = new Vector3(rect.width, rect.height, 1f);
        }

        private void Update()
        {
            var color = _hovered ? _sourceColorBlock.highlightedColor : _sourceColorBlock.normalColor;

            _colorBlock.normalColor = Color.Lerp(_colorBlock.normalColor, color, Time.deltaTime * 10f);
            _colorBlock.colorMultiplier = 1;
            _button.colors = _colorBlock;
        }

        public override void OnCursorLeave()
        {
            _hovered = false; 
        }

        public override void OnCursorHover(Vector3 position)
        {
            _hovered = true;
        }

        public override void OnClick(Vector3 position)
        {
            if(!_interactable)
            {
                return;
            }

            _colorBlock.normalColor = _sourceColorBlock.pressedColor;
            _button.colors = _colorBlock;
            _button.onClick.Invoke();
        }
    }
}