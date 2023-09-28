Shader "Hidden/AdditiveBlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

		// Additive blend
		Blend One One

		Pass
		{
            Name "AdditiveBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Fullscreen.hlsl file provides the vertex shader (FullscreenVert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"

            #pragma vertex FullscreenVert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
			float _AdditiveAmount;

			half4 frag(Varyings input) : SV_Target
			{
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				return color * _AdditiveAmount;
			}
			ENDHLSL
		}
	}
}
