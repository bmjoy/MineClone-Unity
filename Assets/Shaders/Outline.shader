Shader "Unlit/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
		offset -1,-1
		zwrite off
		//ztest always
		GrabPass
		{
			"_Current"
		}
        Pass
        {
		 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return fixed4(1,0,0,1);
            }
            ENDCG
        }
		GrabPass{}
		Pass
		{
				zwrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 scrPos : TEXCOORD0;
			};
			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.scrPos = ComputeScreenPos(o.vertex);
				return o;
			}
			sampler2D _Current, _GrabTexture;
			float4 _GrabTexture_TexelSize;
			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.scrPos.xy / i.scrPos.w;
				float borders = 0;
				borders += tex2D(_GrabTexture, uv + float2(-_GrabTexture_TexelSize.x,0)*1).r;
				borders += tex2D(_GrabTexture, uv + float2(_GrabTexture_TexelSize.x,0) * 1).r;
				borders += tex2D(_GrabTexture, uv + float2(0,-_GrabTexture_TexelSize.y) * 1).r;
				borders += tex2D(_GrabTexture, uv + float2(0, _GrabTexture_TexelSize.y) * 1).r;
				borders = floor( borders * 0.25f+0.01);
				return lerp(fixed4(0,0,0,1), tex2D(_Current, uv), borders);
			}

		ENDCG
		}
    }
}
