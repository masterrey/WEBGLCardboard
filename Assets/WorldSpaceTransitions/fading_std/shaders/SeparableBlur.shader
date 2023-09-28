Shader "Hidden/SeparableGlassBlur" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE

#include "UnityCG.cginc"

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;

		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};

	float4 offsets;

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;//

	v2f vert(appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv.xy = v.texcoord.xy;

		o.uv01 = v.texcoord.xyxy + offsets.xyxy * float4(1, 1, -1, -1);
		o.uv23 = v.texcoord.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		o.uv45 = v.texcoord.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

		return o;
	}

	half4 frag(v2f i) : COLOR{
		half4 color = float4 (0,0,0,0);
		color += 0.40 * tex2D(_MainTex, i.uv);
		color += 0.15 * tex2D(_MainTex, i.uv01.xy);
		color += 0.15 * tex2D(_MainTex, i.uv01.zw);
		color += 0.10 * tex2D(_MainTex, i.uv23.xy);
		color += 0.10 * tex2D(_MainTex, i.uv23.zw);
		color += 0.05 * tex2D(_MainTex, i.uv45.xy);
		color += 0.05 * tex2D(_MainTex, i.uv45.zw);

		return color;
	}

	half4 frag2(v2f i) : COLOR
	{
		half4 color = float4 (0,0,0,0);
		float2 Texel = _MainTex_TexelSize.xy;
		//Texel = float2(1,1);
		float2 uv11 = i.uv - offsets.xy*float2(Texel.x, Texel.y);
		float2 uv12 = i.uv + offsets.xy*float2(Texel.x, Texel.y);
		float2 uv21 = i.uv - offsets.xy*float2(Texel.x, Texel.y)*2;
		float2 uv22 = i.uv + offsets.xy*float2(Texel.x, Texel.y)*2;
		float2 uv31 = i.uv - offsets.xy*float2(Texel.x, Texel.y)*3;
		float2 uv32 = i.uv + offsets.xy*float2(Texel.x, Texel.y)*3;
					
		color += 0.40 * tex2D(_MainTex,  i.uv);
		color += 0.15 * tex2D(_MainTex,  uv11);
		color += 0.15 * tex2D(_MainTex,  uv12);
		color += 0.10 * tex2D(_MainTex,  uv21);
		color += 0.10 * tex2D(_MainTex,  uv22);
		color += 0.05 * tex2D(_MainTex,  uv31);
		color += 0.05 * tex2D(_MainTex,  uv32);

		return color;
	}

	ENDCG

	Subshader {
		Pass{
			 ZTest Always Cull Off ZWrite Off
			 Fog { Mode off }

			 CGPROGRAM
			 #pragma fragmentoption ARB_precision_hint_fastest
			 #pragma vertex vert
			 #pragma fragment frag2
			 ENDCG
		}
	}

	Fallback off


} // shader
