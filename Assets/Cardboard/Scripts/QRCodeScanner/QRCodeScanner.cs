using Google.Protobuf;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using System.Collections.Generic;
using UnityEngine.Android;
using TiltShift.UI;

namespace TiltShift.Cardboard
{
    [Serializable]
    public class UnityEventString : UnityEvent<string> { }

    public class QRCodeScanner : MonoBehaviour
    {
        private WebCamTexture _currentCamTexture;

        public RawImage CamAreaImg;

        public UnityEventString OnPayloadReceived;

        private Thread _readerThread;

        private Color32[] _prevColors;

        private int _prevHeight;
        private int _prevWidth;

        private string _qrCodePayload = string.Empty;

        private bool _isBusy = false;

        public GameObject LoadingObj;

        public RectTransform FocusObject;

        public float AreaPercentSize = 0.7f;

        public RectTransform AreaScanRect;

        public GameObject OpenSettingsBlock;

        public GameObject NotifyBlock;

        private void Awake()
        {
#if UNITY_IOS
            if(Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                OpenSettingsBlock.SetActive(false);
            }

#elif UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                OpenSettingsBlock.SetActive(false);
            }
#endif
        }

        private void Start()
        {
            StartCoroutine(StartCamera());
            _readerThread = new Thread(DecodeQR);
            _readerThread.Start();
        }

        private IEnumerator StartCamera()
        {
#if UNITY_IOS
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

            while (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return null;
            }
                        
#elif UNITY_ANDROID

            Permission.RequestUserPermission(Permission.Camera);

            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                yield return null;
            }
#endif
            _currentCamTexture = new WebCamTexture();
            _currentCamTexture.requestedHeight = Screen.height;
            _currentCamTexture.requestedWidth = Screen.width;
            _currentCamTexture.Play();

            while (!_currentCamTexture.isPlaying)
            {
                yield return null;
            }

            Debug.Log($"QRCodeReader: Init cam - {_currentCamTexture.width}x{_currentCamTexture.height}x{_currentCamTexture.requestedFPS}");

            var camRatio = (float)_currentCamTexture.width / (float)_currentCamTexture.height;
            Debug.Log($"QRCodeReader: Cam ratio - {camRatio}");

            CamAreaImg.texture = _currentCamTexture;

            FocusObject.gameObject.SetActive(true);
            AreaScanRect.gameObject.SetActive(true);

            OpenSettingsBlock.SetActive(false);
            NotifyBlock?.SetActive(true);

        }

        public void Close()
        {
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            _currentCamTexture?.Stop();
            _readerThread.Abort();
        }

        private void ConvertToColor(Color[] color)
        {
            if (_prevColors == null || color.Length != _prevColors.Length)
            {
                _prevColors = new Color32[color.Length];
            }

            for (long i = 0; i < color.Length; i++)
            {
                _prevColors[i].r = (byte)(color[i].r * 255f);
                _prevColors[i].g = (byte)(color[i].g * 255f);
                _prevColors[i].b = (byte)(color[i].b * 255f);
                _prevColors[i].a = (byte)(color[i].a * 255f);
            }
        }

        private void Update()
        {
            LoadingObj?.SetActive(_isBusy);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }

            if (_currentCamTexture == null || !_currentCamTexture.isPlaying)
            {
                return;
            }

            CheckFocus();

            UpdateTexture();

            var minCamSize = _currentCamTexture.width < _currentCamTexture.height ? _currentCamTexture.width : _currentCamTexture.height;
            var minAreaSize = CamAreaImg.rectTransform.rect.width < CamAreaImg.rectTransform.rect.height ?
                CamAreaImg.rectTransform.rect.width :
                CamAreaImg.rectTransform.rect.height;

            ConvertToColor(_currentCamTexture.GetPixels((int)(_currentCamTexture.width / 2f - minCamSize * AreaPercentSize / 2f),
                (int)(_currentCamTexture.height / 2f - minCamSize * AreaPercentSize / 2f),
                (int)(minCamSize * AreaPercentSize), (int)(minCamSize * AreaPercentSize)));

            AreaScanRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minAreaSize * AreaPercentSize);
            AreaScanRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minAreaSize * AreaPercentSize);

            _prevHeight = _prevWidth = (int)(minCamSize * AreaPercentSize);

            if (!string.IsNullOrEmpty(_qrCodePayload))
            {
                StartCoroutine(DecodeQRCode(_qrCodePayload));
                _qrCodePayload = null;
            }
        }

        private void CheckFocus()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CamAreaImg.rectTransform,
                Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out var localpoint))
            {
                var relativePoint = new Vector2(localpoint.x / CamAreaImg.rectTransform.rect.width * _currentCamTexture.width + _currentCamTexture.width / 2f,
                    _currentCamTexture.height - (localpoint.y / CamAreaImg.rectTransform.rect.height * _currentCamTexture.height + _currentCamTexture.height / 2f));

                Debug.Log($"QRCodeReader: New focus point {relativePoint}");

                FocusObject.localPosition = localpoint;
                _currentCamTexture.autoFocusPoint = relativePoint;

                var animFocus = FocusObject.GetComponent<ZoomAnimation>();

                if (animFocus != null)
                {
                    animFocus.StartAnim();
                }
            }
        }

        private void UpdateTexture()
        {
            if (_currentCamTexture.width < 100)
            {
                Debug.Log("QRCodeScanner: Still waiting another frame for correct info...");
                return;
            }

            // change as user rotates iPhone or Android:

            int cwNeeded = _currentCamTexture.videoRotationAngle;
            // Unity helpfully returns the _clockwise_ twist needed
            // guess nobody at Unity noticed their product works in counterclockwise:
            int ccwNeeded = -cwNeeded;

            // IF the image needs to be mirrored, it seems that it
            // ALSO needs to be spun. Strange: but true.
            if (_currentCamTexture.videoVerticallyMirrored) ccwNeeded += 180;

            // you'll be using a UI RawImage, so simply spin the RectTransform
            CamAreaImg.rectTransform.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

            float videoRatio = (float)_currentCamTexture.width / (float)_currentCamTexture.height;
            AspectRatioFitter rawImageARF = CamAreaImg.GetComponentInParent<AspectRatioFitter>();
            rawImageARF.aspectRatio = videoRatio;

            if (_currentCamTexture.videoVerticallyMirrored)
                CamAreaImg.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
            else
                CamAreaImg.uvRect = new Rect(0, 0, 1, 1);  // means no flip
        }

        private void DecodeQR()
        {
            var barcodeReader = new BarcodeReader();
            barcodeReader.AutoRotate = true;
            barcodeReader.TryInverted = true;
            barcodeReader.Options.PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE };

            while (true)
            {
                try
                {
                    if (!_isBusy)
                    {
                        // decode the current frame
                        var result = barcodeReader.Decode(_prevColors, _prevWidth, _prevHeight);

                        if (result != null)
                        {
                            _qrCodePayload = result.Text;
                        }
                    }

                    // Sleep a little bit and set the signal to get the next frame
                    Thread.Sleep(100);
                }
                catch
                {
                }
            }
        }

        private string ModernUrl(string url)
        {
            if (!url.Contains("http://") && !url.Contains("https://"))
            {
                return $"https://{url}";
            }

            return url;
        }

        private string FixPayload(string payload)
        {
            var padding = 4 - payload.Length % 4;

            var result = new StringBuilder(payload);
            result.Append('=', padding == 4 ? 0 : padding);
            result.Replace('-', '+');

            return result.ToString();
        }

        private IEnumerator DecodeQRCode(string result)
        {
            _isBusy = true;

            using (var request = UnityWebRequest.Head(new Uri(ModernUrl(result))))
            {
                yield return request.SendWebRequest();

                Debug.Log($"QRCodeScanner: Response : {request.url}");

                var keyString = "?p=";
                var index = request.url.IndexOf(keyString);
                index += keyString.Length;

                var sourcePayload = request.url.Substring(index, request.url.Length - index);

                Debug.Log($"QRCodeScanner: Source payload : {sourcePayload} Len : {sourcePayload.Length}");

                var payload = FixPayload(sourcePayload);

                Debug.Log($"QRCodeScanner: Getting payload : {payload} Len : {payload.Length}");

                try
                {
                    var bytes = ByteString.FromBase64(payload);
                    OnPayloadReceived?.Invoke(payload);
                    Destroy(gameObject);
                }
                catch (Exception)
                {
                    Debug.LogWarning("QRCodeScanner: Error decoding payload!");
                }

                _isBusy = false;
            }
        }
    }
}