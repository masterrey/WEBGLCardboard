Shader "Hidden/GlassBlur" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

		// Additive blend
		//Blend One One

		Pass
		{
            Name "AdditiveBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            // The Fullscreen.hlsl file provides the vertex shader (FullscreenVert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"

            #pragma vertex FullscreenVert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float4 offsets;

			half4 frag(Varyings input) : SV_Target
			{
				half4 color = float4 (0,0,0,0);
				float2 Texel = _MainTex_TexelSize.xy;
				//Texel = float2(1,1);
				float2 uv11 = input.uv - offsets.xy*float2(Texel.x, Texel.y);
				float2 uv12 = input.uv + offsets.xy*float2(Texel.x, Texel.y);
				float2 uv21 = input.uv - offsets.xy*float2(Texel.x, Texel.y)*2;
				float2 uv22 = input.uv + offsets.xy*float2(Texel.x, Texel.y)*2;
				float2 uv31 = input.uv - offsets.xy*float2(Texel.x, Texel.y)*3;
				float2 uv32 = input.uv + offsets.xy*float2(Texel.x, Texel.y)*3;
					
				color += 0.40 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, input.uv);
				color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv11);
				color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv12);
				color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv21);
				color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv22);
				color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv31);
				color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv32);

				return color;
			}

			ENDHLSL
		}
	}
} // shader
