Shader "Unlit/Skybox"
{
	Properties 
	{
		_ColorTop ("Color Top", Color) = (1,1,1,1)
		_ColorHorizon("Color Horizon", Color) = (0,0,1,1)
		_ColorBottom("Color Bottom", Color) = (0,0,0,1)

	}

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
			uniform fixed4 _ColorHorizon, _ColorTop, _ColorBottom;
			fixed4 frag (v2f i) : SV_Target
			{
				float3 d = normalize(i.texcoord);
				float height = pow( 2*atan2((abs(d.y)),sqrt(d.x*d.x + d.z*d.z))/3.141592,1);
				fixed4 c2;
				if (d.y > 0)
				{
					c2 = _ColorTop;
				}
				else {
					c2 = _ColorBottom;
				}

				return lerp(_ColorHorizon,c2,height);
			}
			ENDCG
		}
	}
	Fallback Off
}
