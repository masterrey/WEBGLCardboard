using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace Bozo.ModularCharacters
{



    public class ColorPickerControl : MonoBehaviour
    {
        public float currentHue;
        public float currentSat;
        public float currentVal;
        public float currentColor;

        [SerializeField] private RawImage hueImage;
        [SerializeField] private RawImage satValImage;
        [SerializeField] private RawImage outputImage;

        [SerializeField] private Slider hueSlider;

        private Texture2D hueTexture;
        private Texture2D svTexture;
        private Texture2D outputTexture;

        public Renderer colorObject;
        public Material colorMaterial;
        public int MaterialSlot;

        [SerializeField] Text objectName;
        [SerializeField] Image[] Swatches;
        [SerializeField] int currentSwatch;

        private void Start()
        {
            CreateHueImage();
            CreateSVImage();
            CreateOutputImage();
            UpdateOutputImage();
        }

        private void CreateHueImage()
        {
            hueTexture = new Texture2D(1, 16);
            hueTexture.wrapMode = TextureWrapMode.Clamp;
            hueTexture.name = "HueTexture";

            for (int i = 0; i < hueTexture.height; i++)
            {
                hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));
            }

            hueTexture.Apply();

            currentHue = 0;
            hueImage.texture = hueTexture;
        }

        private void CreateSVImage()
        {
            svTexture = new Texture2D(16, 16);
            svTexture.wrapMode = TextureWrapMode.Clamp;
            svTexture.name = "SVTexture";

            for (int y = 0; y < svTexture.height; y++)
            {
                for (int x = 0; x < svTexture.width; x++)
                {
                    svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
                }
            }

            svTexture.Apply();

            currentSat = 0;
            currentVal = 0;
            satValImage.texture = svTexture;
        }

        private void CreateOutputImage()
        {
            outputTexture = new Texture2D(1, 16);
            outputTexture.wrapMode = TextureWrapMode.Clamp;
            outputTexture.name = "OutputTexture";

            Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

            for (int i = 0; i < hueTexture.height; i++)
            {
                outputTexture.SetPixel(0, 1, currentColor);
            }

            outputTexture.Apply();

            currentHue = 0;
            outputImage.texture = outputTexture;
        }

        private void UpdateOutputImage()
        {
            Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

            for (int i = 0; i < outputTexture.height; i++)
            {
                outputTexture.SetPixel(0, i, currentColor);
            }

            outputTexture.Apply();

            if (!colorObject) return;

            Swatches[currentSwatch].color = currentColor;

            colorMaterial.SetColor("_Color_" + (currentSwatch + 1), currentColor);

            colorObject.materials[MaterialSlot] = colorMaterial;
        }

        public void SetSV(float S, float V)
        {
            currentSat = S;
            currentVal = V;
            UpdateOutputImage();
        }

        public void SetHSV(float H, float S, float V)
        {
            currentHue = H;
            currentSat = S;
            currentVal = V;
            UpdateOutputImage();
        }

        public void UpdateSVImage()
        {
            currentHue = hueSlider.value;

            for (int y = 0; y < svTexture.height; y++)
            {
                for (int x = 0; x < svTexture.width; x++)
                {
                    svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));

                }
            }

            svTexture.Apply();
            UpdateOutputImage();
        }

        public void ChangeSwatch(int value)
        {
            currentSwatch = value;
            var swatchColor = Swatches[currentSwatch].color;
            Color.RGBToHSV(swatchColor, out float h, out float s, out float v);
            hueSlider.value = h;
            SetHSV(h, s, v);
            UpdateSVImage();
        }

        public void ChangeObject(Transform transform)
        {
            colorObject = transform.GetComponentInChildren<Renderer>();
            if (colorObject == null) return;

            for (int i = 0; i < colorObject.materials.Length; i++)
            {
                var sort = colorObject.materials[i].name.Split("_");

                if (sort[1] == "Outfit")
                {
                    colorMaterial = colorObject.materials[i];
                    MaterialSlot = i;
                    break;
                }
                else
                {
                    colorMaterial = null;
                }
            }

            if (colorMaterial == null) return;

            for (int i = 0; i < Swatches.Length; i++)
            {
                Swatches[i].color = colorMaterial.GetColor("_Color_" + (i + 1));
            }

            ChangeSwatch(0);

            objectName.text = colorObject.name.Replace("(Clone)", "");

        }
    }
}
