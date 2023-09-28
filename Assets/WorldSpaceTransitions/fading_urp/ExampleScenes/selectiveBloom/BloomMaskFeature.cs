using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



// This class sets up the bloom pass
public class BloomMaskFeature : ScriptableRendererFeature
{

    // This class implments the bloom effect
    class BloomMaskPass : ScriptableRenderPass
    {
        //public const int k_PerObjectBlurRenderLayerIndex = 5;

        private const string k_PerObjectBloomTag = "_PerObjectBloomMask";

        static readonly string kStencilWriteShaderName = "Hidden/Internal-StencilWrite";
        static readonly ShaderTagId kLightweightForwardShaderId = new ShaderTagId("UniversalForward");

        RenderTargetHandle m_PerObjectRenderTextureHandle;
        FilteringSettings m_PerObjectFilterSettings;
        Material maskMaterial = null;

        public BloomMaskPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
        {
            // Setup a target RT handle (it just wraps the int id)
            m_PerObjectRenderTextureHandle.Init(k_PerObjectBloomTag);

            m_PerObjectFilterSettings = new FilteringSettings(renderQueueRange, layerMask);

            // This just writes black values for anything that is rendered
            this.maskMaterial = material;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(m_PerObjectRenderTextureHandle.id, cameraTextureDescriptor);

            ConfigureTarget(m_PerObjectRenderTextureHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.white);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_PerObjectBloomTag);

            using (new ProfilingSample(cmd, k_PerObjectBloomTag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var camera = renderingData.cameraData.camera;

                // We want the same rendering result as the main opaque render
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;

                // Setup render data from camera
                var drawSettings = CreateDrawingSettings(kLightweightForwardShaderId, ref renderingData, sortFlags);
                drawSettings.overrideMaterial = maskMaterial;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_PerObjectFilterSettings);

                // Set a global texture id so we can access this later on
                cmd.SetGlobalTexture("_PerObjectBloomMask", m_PerObjectRenderTextureHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);

            // When rendering is done, clean up our temp RT
            cmd.ReleaseTemporaryRT(m_PerObjectRenderTextureHandle.id);
        }
    }
    

   [System.Serializable]
    public class MaskSettings
    { 
        public LayerMask layermask;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Shader maskShader;
    }

    public MaskSettings settings;
    Material maskMaterial;

    BloomMaskPass m_perObjectPass;

    public override void Create()
    {
        if (settings.maskShader != null) // to explain: why always null ?
        {
            maskMaterial = CoreUtils.CreateEngineMaterial(settings.maskShader);
        }
        else
        {
            //Debug.LogWarningFormat("Missing mask Shader");
            maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-StencilWrite");
        }
        m_perObjectPass = new BloomMaskPass(RenderQueueRange.all, settings.layermask, maskMaterial);
        m_perObjectPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_perObjectPass);
    }
}


