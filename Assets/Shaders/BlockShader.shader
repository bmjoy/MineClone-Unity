Shader "Custom/DefaultMat"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Block

		  half4 LightingBlock(SurfaceOutput s, half3 lightDir, half atten) {
			  half NdotL = dot(s.Normal, lightDir);
			  half4 c;
			  NdotL = max(NdotL,0.1);
			  c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
			  
			  c.a = 1;
			  return c;
		  }

        sampler2D _MainTex;
		uniform sampler2D _BlockTextures;

        struct Input
        {
            float2 uv_BlockTextures;
			float4 color : COLOR;
			float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        { 
			float2 uv = IN.uv_BlockTextures;
			uv = uv/512.0;
			uv.y = 1 - uv.y;
            fixed4 c = tex2D (_BlockTextures, uv);
			clip(c.a-0.1);
			float fade = pow(distance(_WorldSpaceCameraPos.xz, IN.worldPos.xz) /(16.0 - 1.0)/16.0,12);
			o.Albedo = c.rgb * IN.color.rgb;
			o.Emission = fade;
			o.Gloss = 0;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
