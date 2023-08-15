using UnityEngine;

namespace TiltShift.UI
{
    public class ZoomAnimation : MonoBehaviour
    {
        private Vector3 _baseLocalScale;

        public float ZoomCoef = 1.5f;

        private void Awake()
        {
            _baseLocalScale = transform.localScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _baseLocalScale, Time.deltaTime);                
        }

        public void StartAnim()
        {
            transform.localScale = _baseLocalScale * ZoomCoef;
        }
    }

}