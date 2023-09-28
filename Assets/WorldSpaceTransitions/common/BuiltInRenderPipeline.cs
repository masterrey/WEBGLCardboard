using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    //switch off other render pipelines when opening a scene for the built in renderpipeline.
    public class BuiltInRenderPipeline : MonoBehaviour
    {
        void OnEnable()
        {
            GraphicsSettings.renderPipelineAsset = null;
            QualitySettings.renderPipeline = null;
        }
    }
}