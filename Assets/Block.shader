//https://github.com/przemyslawzaworski/Unity3D-CG-programming/blob/master/deferred_metallic_gloss.shader

Shader "Unlit/Block"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags {"LightMode" = "Deferred"}

			CGPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma exclude_renderers nomrt
			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma target 3.0
			#include "UnityPBSLighting.cginc"

			float4 _Color;
			float _Metallic;
			float _Gloss;

			struct structureVS
			{
				float4 screen_vertex : SV_POSITION;
				float4 world_vertex : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float4 color : COLOR;
			};

			struct structurePS
			{
				half4 albedo : SV_Target0;
				half4 specular : SV_Target1;
				half4 normal : SV_Target2;
				half4 emission : SV_Target3;
			};

			structureVS vertex_shader(float4 vertex : POSITION,float3 normal : NORMAL, float2 uv : TEXCOORD0, float4 color : COLOR)
			{
				structureVS vs;
				vs.screen_vertex = UnityObjectToClipPos(vertex);
				vs.world_vertex = mul(unity_ObjectToWorld, vertex);
				vs.normal = UnityObjectToWorldNormal(normal);
				vs.uv= uv / 512.0;
				vs.uv.y = 1 - vs.uv.y;
				vs.color = color;
				//vs.uv = uv;
				return vs;
			}

			uniform fixed4 _ColorHorizon, _ColorTop, _ColorBottom;
			fixed4 GetSkyColor(float3 viewDir)
			{
				float2 lat = atan2((abs(viewDir.y)), sqrt(viewDir.x*viewDir.x + viewDir.z*viewDir.z));
				float height = pow(2 * lat / 3.141592, 1);
				return lerp(_ColorHorizon, lerp(_ColorBottom, _ColorTop, saturate(sign(viewDir.y))), height);
			}

			uniform sampler2D _BlockTextures;
			structurePS pixel_shader(structureVS vs)
			{
				structurePS ps;
				float3 normalDirection = normalize(vs.normal);
				half3 specular;
				half specularMonochrome;
				half3 diffuseColor = DiffuseAndSpecularFromMetallic(_Color.rgb, _Metallic, specular, specularMonochrome);

				fixed4 c = tex2D(_BlockTextures, vs.uv);
				//c.rgb = 1;
				clip(c.a - 0.1);

				fixed4 sky = GetSkyColor(vs.world_vertex - _WorldSpaceCameraPos);
				float fade = saturate(pow(distance(_WorldSpaceCameraPos.xz, vs.world_vertex.xz) / (16.0 - 1.0) / 16.0, 12));
				float4 vertexColor = vs.color;
				c.rgb *= vertexColor.rgb;
				float lightLevel = vertexColor.a * 16;
				float light = lerp(0.1, 1, lightLevel);
				
				c *= light;
				c.rgb += diffuseColor;

				ps.albedo = 0;
				ps.specular = half4(specular, 0);
				ps.normal = half4(normalDirection * 0.5 + 0.5, 1.0);
				ps.emission = lerp( c,sky, fade);
				#ifndef UNITY_HDR_ON
					ps.emission.rgb = exp2(-ps.emission.rgb);
				#endif
				return ps;
			}
            ENDCG
        }
    }
}
