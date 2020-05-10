Shader "Unlit/Skybox"
{
	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.vertex.xyz;
				return o;
			}

			uniform fixed4 _SkyColorHorizon, _SkyColorTop, _SkyColorBottom;
			fixed4 GetSkyColor(float3 viewDir)
			{
				float2 lat = atan2((abs(viewDir.y)), sqrt(viewDir.x*viewDir.x + viewDir.z*viewDir.z));
				float height = pow(2 * lat / 3.141592, 1);
				return lerp(_SkyColorHorizon, lerp(_SkyColorBottom, _SkyColorTop, saturate(sign(viewDir.y))), height);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 viewDir = normalize(i.texcoord);
				

				return GetSkyColor(viewDir);
			}
			ENDCG
		}
	}
	Fallback Off
}
