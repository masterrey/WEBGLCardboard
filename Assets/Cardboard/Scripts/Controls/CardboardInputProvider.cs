using UnityEngine;
using UnityEngine.UI;
using SojaExiles;

namespace TiltShift.Cardboard.Controls
{
    public class CardboardInputProvider : MonoBehaviour
    {
        private UnityEngine.Camera _currentCamera;

        public static CardboardInputProvider Instance;

        private RaycastHit _hit;
        private Ray _ray;

        private float _currentTimeAwaitingClick = 0f;

        public bool GazeClicking = true;

        public float TimeAwaitingClick = 1f;

        public Image LoadingImage;

        public CardboardControlBase ActiveInteractableObject;

        public Canvas CursorCanvas;

        private void Awake()
        {
            _currentCamera = GetComponent<UnityEngine.Camera>();
            Instance = this;
        }

        private void UpdateCursor()
        {
            var transformPosition = CursorCanvas.transform.localPosition;

            var destZ = _hit.collider == null ? 4f : Mathf.Clamp(Vector3.Distance(transform.position, _hit.point) - 0.1f, 1f, 6f);

            transformPosition.z = destZ;

            LoadingImage.fillAmount = ActiveInteractableObject == null ? 0 : _currentTimeAwaitingClick / TimeAwaitingClick;

            CursorCanvas.transform.localPosition = transformPosition;
        }

        private void FixedUpdate()
        {
            UpdateCursor();

            var prevObj = ActiveInteractableObject;

            ActiveInteractableObject = null;

            _ray = _currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            if (!Physics.Raycast(_ray, out _hit, Mathf.Infinity))
            {
                _currentTimeAwaitingClick = 0;
                prevObj?.OnCursorLeave();
                return;
            }

            var cardboardControl = _hit.collider.gameObject.GetComponent<CardboardControlBase>();
            if (cardboardControl == null)
            {
                var opencloseDoor = _hit.collider.gameObject.GetComponent<opencloseDoor>();
                if (opencloseDoor != null)
                {
                    opencloseDoor.ForceOpening();
                }

                var agentMove = _hit.collider.gameObject.GetComponent<AgentMove>();
                if (agentMove != null)
                {
                    agentMove.moveto();
                }

            }

            if (GazeClicking)
            {
                UpdateGazeClicking(prevObj, cardboardControl);
            }
            else
            {
                UpdateStandardClicking(prevObj, cardboardControl);
            }

            ActiveInteractableObject = cardboardControl;
        }

        private void UpdateGazeClicking(CardboardControlBase prevObj, CardboardControlBase cardboardControl)
        {
            if (cardboardControl == null)
            {
                prevObj?.OnCursorLeave();
                _currentTimeAwaitingClick = 0;
                return;
            }

            if (prevObj == cardboardControl && !cardboardControl.IgnoreClick)
            {
                _currentTimeAwaitingClick += Time.fixedDeltaTime;
            }
            else
            {
                cardboardControl.OnCursorHover(cardboardControl.transform.InverseTransformPoint(_hit.point));
                _currentTimeAwaitingClick = 0;
            }

            if (_currentTimeAwaitingClick > TimeAwaitingClick)
            {
                cardboardControl.OnClick(cardboardControl.transform.InverseTransformPoint(_hit.point));
                _currentTimeAwaitingClick = 0;
            }
        }

        private void UpdateStandardClicking(CardboardControlBase prevObj, CardboardControlBase cardboardControl)
        {
            _currentTimeAwaitingClick = 0;

            if (cardboardControl == null)
            {
                prevObj?.OnCursorLeave();
                return;
            }

            cardboardControl.OnCursorHover(cardboardControl.transform.InverseTransformPoint(_hit.point));

            if (Input.GetMouseButtonDown(0))
            {
                cardboardControl.OnClick(cardboardControl.transform.InverseTransformPoint(_hit.point));
            }

        }

    }
}