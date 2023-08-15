using UnityEngine;

namespace TiltShift.Cardboard.Controls.Demo
{
    public class BoxHighLighting : CardboardControlBase
    {
        private Material _mat;

        public Color HighlightedColor;

        public Color NormalColor;

        private Color _currentColor;

        private void Awake()
        {
            base.IgnoreClick = true;

            _mat = new Material(Shader.Find("UI/Default"));

            _currentColor = NormalColor;
            
            GetComponent<MeshRenderer>().material = _mat;
        }

        private void Update()
        {
            _mat.color = Color.Lerp(_mat.color, _currentColor, Time.deltaTime * 5f);            
        }

        public override void OnCursorHover(Vector3 position)
        {
            _currentColor = HighlightedColor;                        
        }

        public override void OnCursorLeave()
        {
            _currentColor = NormalColor;
        }
    }
}
