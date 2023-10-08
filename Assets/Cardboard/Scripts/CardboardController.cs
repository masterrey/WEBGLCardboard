using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;

namespace TiltShift.Cardboard
{
    public class CardboardController : MonoBehaviour
    {
        [SerializeField] private Button OpenQrCodeScannerButton;
        [SerializeField] private Button ChangeCameraModeButton;
        [SerializeField] private Button ExitButton;

        [SerializeField] private UnityEngine.Camera LeftEyeCam;
        [SerializeField] private UnityEngine.Camera RightEyeCam;
        [SerializeField] private UnityEngine.Camera BothCam;
        [SerializeField] private UnityEngine.Camera MiddleCam;
        [SerializeField] TextAsset cardboardpayload;
        [SerializeField] float manualzoom;

        public string ShaderName = "UI/Default";

        public QRCodeScanner QRCodeScannerPrefab;

        public List<GameObject> ObjectsForHidden;

        [Range(0.1f, 5f)] public float RenderTextureMultiplicator = 2f;

        public float ViewDPI => Util.GetDPI();



        private Material _leftEyeMaterial;
        private Material _rightEyeMaterial;
        private RenderTexture _leftEyeRT => LeftEyeCam.targetTexture;
        private RenderTexture _rightEyeRT => RightEyeCam.targetTexture;

        #region Prev orientation states.

        private bool _autorotateToLandscapeLeft;
        private bool _autorotateToLandscapeRight;
        private bool _autorotateToPortraitUpSide;
        private bool _autorotateToPortrait;

        #endregion

        private int _screenTimeOut;

        private float _prevRatio = 0;

        private string PathToSettings => Path.Combine(Application.persistentDataPath, _filePresetName);

        private LensDistortion _lensDistortion;

        private string _qrPayload =
            "CgZIb21pZG8SDUhvbWlkbyAibWluaSIdhxZZPSW28309KhAAAEhCAABIQgAASEIAAEhCWAE1KVwPPToIexQuPs3MTD1QAGAC";

        private string _filePresetName = "cardboardPayload";
        private int ViewWidth => Screen.width;
        private int ViewHeight => Screen.height;

        private float Ratio => ViewWidth / ViewHeight * Util.GetDPI();

        private bool isOneCameraMode = false;

        private RenderMode _currentRenderMode;
        private void Awake()
        {
            LoadLastQrCode();

            manualzoom = PlayerPrefs.GetFloat("manualzoom", 1.4f);
        }

        private void OnEnable()
        {

            SubscribeButton();
#if UNITY_PIPELINE_URP || UNITY_PIPELINE_HDRP
            // code for URP or HDRP
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
#endif
            Application.targetFrameRate = 60;
            _screenTimeOut = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _autorotateToLandscapeLeft = Screen.autorotateToLandscapeLeft;
            _autorotateToLandscapeRight = Screen.autorotateToLandscapeRight;
            _autorotateToPortrait = Screen.autorotateToPortrait;
            _autorotateToPortraitUpSide = Screen.autorotateToPortraitUpsideDown;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            foreach (var obj in ObjectsForHidden)
            {
                obj.SetActive(false);
            }
            Debug.Log("ViewWidth: " + ViewWidth);
        }

        private void OnDisable()
        {
            UnSubscribeButton();
#if UNITY_PIPELINE_URP || UNITY_PIPELINE_HDRP
            // code for URP or HDRP
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
#endif
            Screen.sleepTimeout = _screenTimeOut;
            Screen.autorotateToLandscapeLeft = _autorotateToLandscapeLeft;
            Screen.autorotateToLandscapeRight = _autorotateToLandscapeRight;
            Screen.autorotateToPortrait = _autorotateToPortrait;
            Screen.autorotateToPortraitUpsideDown = _autorotateToPortraitUpSide;

            foreach (var obj in ObjectsForHidden)
            {
                obj.SetActive(true);
            }
        }

        private void LoadLastQrCode()
        {

            if (cardboardpayload != null)
            {
                var payload = cardboardpayload.text;
                _qrPayload = payload;
                Debug.Log("Cardboard: QRCode loaded");


            }
            /*
           try
           {

               var payload = File.ReadAllText(PathToSettings);
               Debug.Log(PathToSettings);
               _qrPayload = payload;
               Debug.Log("Cardboard: QRCode loaded");
           }
           catch (FileNotFoundException)
           {
               Debug.LogWarning($"Cardboard: Saved QRCode is not exist. ");
           }
           */
            SetPayload(_qrPayload);
        }

        private void SetPayload(string payload)
        {
            _qrPayload = payload;

            _lensDistortion = new LensDistortion(_qrPayload, ViewWidth, ViewHeight, ViewDPI);

            InitCameras();

            InitMaterials();

            File.WriteAllText(PathToSettings, payload);
        }

        private void SubscribeButton()
        {
            OpenQrCodeScannerButton.onClick.AddListener(OpenQrCodeScanner);
            ChangeCameraModeButton.onClick.AddListener(ChangeCameraMode);
            ExitButton.onClick.AddListener(Exit);
        }

        private void UnSubscribeButton()
        {
            OpenQrCodeScannerButton.onClick.RemoveListener(OpenQrCodeScanner);
            ChangeCameraModeButton.onClick.RemoveListener(ChangeCameraMode);
            ExitButton.onClick.RemoveListener(Exit);
        }


        private void InitCameras()
        {
            InitTargetTextures();

            BothCam.projectionMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, -0.1f, 0.5f);

            LeftEyeCam.transform.localPosition = _lensDistortion.GetEyePosition(CardboardEye.Left);
            RightEyeCam.transform.localPosition = _lensDistortion.GetEyePosition(CardboardEye.Right);

            LeftEyeCam.projectionMatrix = _lensDistortion.GetProjectionMatrix(CardboardEye.Left);
            RightEyeCam.projectionMatrix = _lensDistortion.GetProjectionMatrix(CardboardEye.Right);

            Debug.Log("Cardboard: Camera's initialized");
        }

        private void InitTargetTextures()
        {
            var size = new Vector2(ViewWidth / 2f * 2f, ViewHeight);

            var leftRt = new RenderTexture((int)size.x, (int)size.y, 24, GraphicsFormat.R8G8B8A8_UNorm, 2);
            var rightRt = new RenderTexture((int)size.x, (int)size.y, 24, GraphicsFormat.R8G8B8A8_UNorm, 2);

            leftRt.antiAliasing = rightRt.antiAliasing = 2;

            LeftEyeCam.targetTexture = leftRt;
            RightEyeCam.targetTexture = rightRt;
            Debug.Log("Cardboard: target initialized");
        }

        private void InitMaterials()
        {
            _leftEyeMaterial = new Material(Shader.Find(ShaderName));
            _rightEyeMaterial = new Material(Shader.Find(ShaderName));

            Debug.Log("Cardboard: Materials initialized");
        }

        private void Update()
        {
           // pinch();
            if (Mathf.Abs(_prevRatio - Ratio) < 0.001f)
            {
                return;
            }

            UpdateSizes();

        }

        private void UpdateSizes()
        {
            _prevRatio = Ratio;
            _lensDistortion.UpdateSize(ViewWidth, ViewHeight, ViewDPI);
            InitCameras();

           // Debug.Log("Cardboard: Reinit graphics");
        }

        //using 2 fingers to change the zoom in and out in the manualzoom variable

        public float zoomSpeed = 0.1f; // The speed of the zoom



        void pinch()
        {
            if (Input.GetButtonDown("Fire1")) // check that there are two touches
            {

                manualzoom += zoomSpeed;
                Debug.Log("zooming in " + manualzoom);
                PlayerPrefs.SetFloat("manualzoom", manualzoom);
                PlayerPrefs.Save();
                // Optionally clamp the zoom level, e.g., between a min and max value
                if (manualzoom > 1.7f)
                {
                    manualzoom = 1;
                }
                manualzoom = Mathf.Clamp(manualzoom, 1f, 2f);



                UpdateSizes();
            }
        }


        private void OnEndCameraRendering(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            OnPostRender();
        }
        private void OnPostRender()
        {
            if (_leftEyeRT == null || _rightEyeRT == null)
            {
                return;
            }

            _leftEyeMaterial.mainTexture = _leftEyeRT;
            _rightEyeMaterial.mainTexture = _rightEyeRT;

            var _transform = transform;
            var position = _transform.position;
            var rotation = _transform.rotation;

            _leftEyeMaterial.SetPass(0);

            Graphics.DrawMeshNow(_lensDistortion.GetDistortionMesh(CardboardEye.Left), position, rotation);

            _rightEyeMaterial.SetPass(0);
            Graphics.DrawMeshNow(_lensDistortion.GetDistortionMesh(CardboardEye.Right), position, rotation);
        }

        private void OpenQrCodeScanner()
        {
            var qrCodeScanner = Instantiate(QRCodeScannerPrefab);
            qrCodeScanner.OnPayloadReceived.AddListener(SetPayload);
        }

        private void ChangeCameraMode()
        {
            MiddleCam.gameObject.SetActive(!MiddleCam.gameObject.activeSelf);
        }

        private void Exit()
        {
            Application.Quit();
        }
    }
}