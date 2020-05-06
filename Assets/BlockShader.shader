Shader "Custom/DefaultMat"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		uniform sampler2D _BlockTextures;

        struct Input
        {
            float2 uv_BlockTextures;
			float4 color : COLOR;
			float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			float2 uv = IN.uv_BlockTextures;
			uv = uv/512.0;
			uv.y = 1 - uv.y;
            fixed4 c = tex2D (_BlockTextures, uv);
			clip(c.a-0.1);
			float fade = pow(distance(_WorldSpaceCameraPos.xz, IN.worldPos.xz) /(16.0 - 1.0)/16.0,12);
			o.Albedo = c.rgb * IN.color.rgb;
			//o.Albedo = 0;
			o.Emission = fade;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			//o.Albedo = float3(uv, 0);
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
