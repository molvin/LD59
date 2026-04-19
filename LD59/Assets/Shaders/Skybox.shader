Shader "Custom/Skybox"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.3, 0.6, 1.0, 1)
        _SunColor ("Sun Color", Color) = (1.0, 0.95, 0.8, 1)
        _SunSize ("Sun Size", Range(0.001, 0.2)) = 0.05
        _SunBloom ("Sun Bloom", Range(0.0, 1.0)) = 0.3
        _SunPixels ("Sun Pixel Size", Range(2, 64)) = 16
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            float4 _SkyColor;
            float4 _SunColor;
            float _SunSize;
            float _SunBloom;
            float _SunPixels;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);

                // Quantize direction to give the sun a pixelated blocky appearance
                float3 pixDir = floor(dir * _SunPixels) / _SunPixels;
                pixDir = normalize(pixDir);

                float sunDirDot = dot(pixDir, sunDir);
                float sunDisk = smoothstep(1.0 - _SunSize, 1.0 - _SunSize * 0.1, sunDirDot);
                float bloom = pow(max(sunDirDot, 0.0), 8.0) * _SunBloom;

                float3 col = _SkyColor.rgb;
                col = lerp(col, _SunColor.rgb, saturate(bloom));
                col = lerp(col, _SunColor.rgb, sunDisk);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
